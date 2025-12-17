using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.Fornecedores;
using OPS.Infraestrutura.Factories;
using Mapster;
using OPS.Importador.Fornecedores.ReceitaWS;
using OPS.Importador.Fornecedores.Mapping;

namespace OPS.Importador.Fornecedores
{
    public class FornecedorQueryResult
    {
        public string CnpjCpf { get; set; }
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public class Fornecedor
    {
        public ILogger<Fornecedor> logger { get; private set; }

        private readonly AppDbContextFactory dbContextFactory;

        public IConfiguration configuration { get; private set; }

        public HttpClient httpClient { get; }

        public Fornecedor(ILogger<Fornecedor> logger, IConfiguration configuration, IServiceProvider serviceProvider, AppDbContextFactory dbContextFactory)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.dbContextFactory = dbContextFactory;

            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");

            // Configure Mapster mappings
            FornecedorMappingProfile.Configure();
        }

        public async Task ConsultarDadosCNPJ(bool somenteNovos = true)
        {
            var telegramApiToken = configuration["AppSettings:TelegramApiToken"];
            var telegram = new TelegramApi(telegramApiToken);
            var telegraMessage = new Core.Entity.TelegramMessage()
            {
                ChatId = "-1001378778982", // OPS - Alertas
                ParseMode = "html",
                Text = ""
            };

            using (var context = dbContextFactory.CreateDbContext())
            {
                var fornecedores = await ObterFornecedoresParaConsulta(context, somenteNovos);
                if (fornecedores.Count == 0)
                {
                    logger.LogInformation("Não há fornecedores para consultar");
                    return;
                }

                var lookupData = await ObterDadosLookup(context);
                logger.LogInformation("Consultando CNPJ's Local: {Total} itens.", fornecedores.Count);

                int totalImportados = await ProcessarFornecedores(context, fornecedores, lookupData, telegram, telegraMessage);
                logger.LogInformation("{TotalImportados} de {TotalFornecedores} fornecedores processados", totalImportados, fornecedores.Count);
            }
        }

        private async Task<List<FornecedorQueryResult>> ObterFornecedoresParaConsulta(AppDbContext context, bool somenteNovos)
        {
            if (somenteNovos)
            {
                return await context.Fornecedores.FromSqlRaw(
                    @"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                    from fornecedor f
                    left join fornecedor_info fi on f.id = fi.id_fornecedor
                    where char_length(f.cnpj_cpf) = 14
                    and (controle is null or controle NOT IN (0, 1, 2, 3))
                    AND fi.id_fornecedor IS null
                    AND f.cnpj_cpf NOT LIKE '%*%' -- Dados anonimizados
                    order by fi.id_fornecedor asc")
                    .Select(f => new FornecedorQueryResult
                    {
                        CnpjCpf = f.CnpjCpf,
                        Id = (int)f.Id,
                        Nome = f.Nome
                    })
                    .ToListAsync();
            }
            else // Atualizar dados antigos
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                return await context.Fornecedores.FromSqlInterpolated(
                    $@"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                    from fornecedor f
                    left join fornecedor_info fi on f.id = fi.id_fornecedor
                    where char_length(f.cnpj_cpf) = 14
                    and obtido_em < '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd")}'
                    and fi.situacao_cadastral = 'ATIVA'
                    order by fi.id_fornecedor asc")
                    .Select(f => new FornecedorQueryResult
                    {
                        CnpjCpf = f.CnpjCpf,
                        Id = (int)f.Id,
                        Nome = f.Nome
                    })
                    .ToListAsync();
            }
        }

        private async Task<(List<FornecedorAtividade> Atividades, List<FornecedorNaturezaJuridica> NaturezaJuridicas)> ObterDadosLookup(AppDbContext context)
        {
            var lstFornecedoresAtividade = await context.FornecedorAtividades.ToListAsync();
            var lstNaturezaJuridica = await context.FornecedorNaturezaJuridicas.ToListAsync();
            return (lstFornecedoresAtividade, lstNaturezaJuridica);
        }

        private async Task<int> ProcessarFornecedores(AppDbContext context, List<FornecedorQueryResult> fornecedores, (List<FornecedorAtividade> Atividades, List<FornecedorNaturezaJuridica> NaturezaJuridicas) lookupData, TelegramApi telegram, Core.Entity.TelegramMessage telegraMessage)
        {
            int totalImportados = 0;
            int atual = 0;

            foreach (var item in fornecedores)
            {
                atual++;

                if (!validarCNPJ(item.CnpjCpf))
                {
                    InserirControle(3, item.CnpjCpf, "CNPJ Invalido");
                    logger.LogInformation("CNPJ Invalido [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, fornecedores.Count, item.CnpjCpf, item.Nome);
                    continue;
                }

                MinhaReceita.FornecedorInfo fornecedor = null;

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"https://minhareceita.org/{item.CnpjCpf}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        fornecedor = JsonSerializer.Deserialize<MinhaReceita.FornecedorInfo>(responseString);
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            logger.LogInformation(response.ReasonPhrase);
                            System.Threading.Thread.Sleep(60000);
                        }
                        else
                        {
                            fornecedor = await ConsultaCnpjReceitaWs(item.CnpjCpf);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    fornecedor = await ConsultaCnpjReceitaWs(item.CnpjCpf);
                }

                if (fornecedor == null) continue;

                // Use EF Core transaction for this supplier
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    fornecedor.Id = item.Id;
                    fornecedor.ObtidoEm = DateTime.Today;
                    fornecedor.RadicalCnpj = fornecedor.Cnpj.Substring(0, 8);

                    // Delete existing related records using EF Core
                    await context.Database.ExecuteSqlInterpolatedAsync($@"
                        delete from fornecedor_info where id_fornecedor={fornecedor.Id};
                        delete from fornecedor_atividade_secundaria where id_fornecedor={fornecedor.Id};
                        delete from fornecedor_socio where id_fornecedor={fornecedor.Id};
                    ");

                    if (fornecedor.IdAtividadePrincipal > 0)
                    {
                        fornecedor.IdAtividadePrincipal = (int)LocalizaInsereAtividade(context, lookupData.Atividades, fornecedor.IdAtividadePrincipal, fornecedor.AtividadePrincipal);
                    }
                    else
                    {
                        throw new NotImplementedException("Atividade principal não encontrada");
                    }

                    if (fornecedor.IdNaturezaJuridica > 0)
                    {
                        fornecedor.IdNaturezaJuridica = LocalizaInsereNaturezaJuridica(context, lookupData.NaturezaJuridicas, fornecedor.IdNaturezaJuridica.ToString("000-0"), fornecedor.NaturezaJuridica);
                    }
                    else
                    {
                        throw new NotImplementedException("Natureza jurídica não encontrada");
                    }

                    if (fornecedor.MotivoSituacaoCadastral == "SEM MOTIVO")
                        fornecedor.MotivoSituacaoCadastral = null;

                    // Use Mapster for entity mapping
                    var fornecedorInfoEntity = fornecedor.Adapt<FornecedorInfo>();
                    context.FornecedorInfos.Add(fornecedorInfoEntity);

                    await context.Database.ExecuteSqlInterpolatedAsync($@"update fornecedor set nome={fornecedor.RazaoSocial} where id={fornecedor.Id}");

                    // Process secondary activities
                    if (fornecedor.CnaesSecundarios != null)
                    {
                        foreach (var atividadesSecundaria in fornecedor.CnaesSecundarios)
                        {
                            try
                            {
                                var idAtividade = LocalizaInsereAtividade(context, lookupData.Item1, atividadesSecundaria.Id, atividadesSecundaria.Descricao);

                                var atividade = new FornecedorAtividadeSecundaria()
                                {
                                    IdFornecedor = (uint)fornecedor.Id,
                                    IdAtividade = idAtividade
                                };

                                context.FornecedorAtividadesSecundarias.Add(atividade);
                            }
                            catch (MySqlException ex)
                            {
                                if (!ex.Message.Contains("Duplicate entry")) throw;
                            }
                        }
                    }

                    await context.SaveChangesAsync();

                    // Process partners (QSA)
                    if (fornecedor.Qsa != null)
                    {
                        var socios = fornecedor.Qsa.Select(qsa =>
                        {
                            qsa.IdFornecedor = fornecedor.Id;
                            if (qsa.CpfRepresentante == "***000000**")
                                qsa.CpfRepresentante = null;
                            return qsa.Adapt<FornecedorSocio>();
                        }).ToList();

                        context.FornecedorSocios.AddRange(socios);
                        await context.SaveChangesAsync();
                    }

                    totalImportados++;

                    // Check for inactive companies with recent activity
                    if (fornecedor.SituacaoCadastral != "ATIVA" && fornecedor.DataSituacaoEspecial != null)
                    {
                        var ultimaNota = await context.Database.SqlQuery<DateTime?>($@"
SELECT MAX(DATA) as data FROM (
    SELECT MAX(d.data_emissao) AS data FROM cf_despesa d WHERE d.id_fornecedor = {fornecedor.Id}
    UNION ALL
    SELECT MAX(d.data_emissao) FROM sf_despesa d WHERE d.id_fornecedor = {fornecedor.Id}
    UNION ALL
    SELECT MAX(d.data_emissao) FROM cl_despesa d WHERE d.id_fornecedor = {fornecedor.Id}
) tmp
").FirstOrDefaultAsync();

                        if (ultimaNota != null && ultimaNota > Convert.ToDateTime(fornecedor.DataSituacaoEspecial))
                        {
                            logger.LogInformation("Empresa Inativa [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, fornecedores.Count, item.CnpjCpf, item.Nome);

                            telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.org.br/fornecedor/{item.Id}'>{item.CnpjCpf} - {item.Nome}</a>";
                            telegram.SendMessage(telegraMessage);
                        }
                    }

                    await transaction.CommitAsync();
                    InserirControle(0, item.CnpjCpf, "");
                    logger.LogInformation("Atualizando [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, fornecedores.Count, item.CnpjCpf, item.Nome);
                }
                catch (Exception ex)
                {


                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Erro ao processar fornecedor {CNPJ}", item.CnpjCpf);
                    InserirControle(1, item.CnpjCpf, ex.Message);
                }

                // Atualizar dados anonimizados para NULL (ReceitaWS)
                await context.Database.ExecuteSqlRawAsync(@"
                	update fornecedor_info set nome_fantasia=null where nome_fantasia = '' or nome_fantasia = '********';
                	update fornecedor_info set logradouro=null where logradouro = '' or logradouro = '********';
                	update fornecedor_info set numero=null where numero = '' or numero = '********';
                	update fornecedor_info set complemento=null where complemento = '' or complemento = '********';
                	update fornecedor_info set cep=null where cep = '' or cep = '********';
                	update fornecedor_info set bairro=null where bairro = '' or bairro = '********';
                	update fornecedor_info set municipio=null where municipio = '' or municipio = '********';
                	update fornecedor_info set estado=null where estado = '' or estado = '**';
                	update fornecedor_info set endereco_eletronico=null where endereco_eletronico = '' or endereco_eletronico = '********';
                	update fornecedor_info set telefone1=null where telefone1 = '' or telefone1 = '********';
                	update fornecedor_info set telefone2=null where telefone2 = '' or telefone2 = '********';
                	update fornecedor_info set ente_federativo_responsavel=null where ente_federativo_responsavel = '' or ente_federativo_responsavel = '********';
                	update fornecedor_info set motivo_situacao_cadastral=null where motivo_situacao_cadastral = '' or motivo_situacao_cadastral = '********';
                	update fornecedor_info set situacao_especial=null where situacao_especial = '' or situacao_especial = '********';
                ");
            }

            return totalImportados;
        }

        public async Task<MinhaReceita.FornecedorInfo> ConsultaCnpjReceitaWs(string cnpj)
        {
            try
            {
                var url = $"https://www.receitaws.com.br/v1/cnpj/{cnpj}";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    var fornecedorWs = JsonSerializer.Deserialize<FornecedorReceitaWs>(responseString);

                    if (fornecedorWs == null)
                        return null;

                    static DateTime? ParseDateString(string s)
                    {
                        if (string.IsNullOrEmpty(s)) return null;
                        // try common formats
                        if (DateTime.TryParse(s, out var dt)) return dt;
                        var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "yyyy-MM-dd HH:mm:ss" };
                        if (DateTime.TryParseExact(s, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt)) return dt;
                        return null;
                    }

                    static decimal ParseDecimalString(string s)
                    {
                        if (string.IsNullOrWhiteSpace(s)) return 0m;
                        // remove currency symbols and trim
                        s = s.Replace("R$", "").Replace("\u00A0", "").Trim();
                        // try pt-BR then invariant
                        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out var d)) return d;
                        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) return d;
                        // keep digits and punctuation
                        var filtered = new string(s.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
                        if (decimal.TryParse(filtered, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out d)) return d;
                        if (decimal.TryParse(filtered, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) return d;
                        return 0m;
                    }

                    static int ParseCodeToInt(string code)
                    {
                        if (string.IsNullOrEmpty(code)) return 0;
                        var digits = new string(code.Where(char.IsDigit).ToArray());
                        if (int.TryParse(digits, out var v)) return v;
                        return 0;
                    }

                    var info = new MinhaReceita.FornecedorInfo
                    {
                        Cnpj = Utils.RemoveCaracteresNaoNumericos(fornecedorWs.Cnpj),
                        Tipo = fornecedorWs.Tipo,
                        Porte = fornecedorWs.Porte,
                        RazaoSocial = fornecedorWs.Nome,
                        NomeFantasia = fornecedorWs.Fantasia,
                        Logradouro = fornecedorWs.Logradouro,
                        Numero = fornecedorWs.Numero,
                        Complemento = fornecedorWs.Complemento,
                        Cep = fornecedorWs.Cep,
                        Bairro = fornecedorWs.Bairro,
                        Municipio = fornecedorWs.Municipio,
                        UF = fornecedorWs.Uf,
                        Email = fornecedorWs.Email,
                        Telefone1 = fornecedorWs.Telefone,
                        EnteFederativoResponsavel = fornecedorWs.Efr,
                        SituacaoCadastral = fornecedorWs.Situacao,
                        DataSituacaoCadastral = ParseDateString(fornecedorWs.DataSituacao),
                        MotivoSituacaoCadastral = fornecedorWs.MotivoSituacao,
                        SituacaoEspecial = fornecedorWs.SituacaoEspecial,
                        DataSituacaoEspecial = ParseDateString(fornecedorWs.DataSituacaoEspecial),
                        CapitalSocial = ParseDecimalString(fornecedorWs.CapitalSocial),
                        OpcaoPeloMEI = fornecedorWs.Simei?.Optante,
                        DataOpcaoPeloMEI = fornecedorWs.Simei is null ? null : ParseDateString(fornecedorWs.Simei.DataOpcao.ToString()),
                        DataExclusaoMEI = fornecedorWs.Simei is null ? null : ParseDateString(fornecedorWs.Simei.DataExclusao.ToString()),
                        OpcaoPeloSimples = fornecedorWs.Simples?.Optante,
                        DataOpcaoPeloSimples = fornecedorWs.Simples is null ? null : ParseDateString(fornecedorWs.Simples.DataOpcao.ToString()),
                        DataExclusaoSimples = fornecedorWs.Simples is null ? null : ParseDateString(fornecedorWs.Simples.DataExclusao.ToString()),
                        NomeCidadeExterior = null,
                        Pais = null,
                        ObtidoEm = DateTime.UtcNow
                    };

                    // Atividade Principal
                    if (fornecedorWs.AtividadePrincipal != null && fornecedorWs.AtividadePrincipal.Count > 0)
                    {
                        info.IdAtividadePrincipal = ParseCodeToInt(fornecedorWs.AtividadePrincipal[0].Code);
                        info.AtividadePrincipal = fornecedorWs.AtividadePrincipal[0].Text;
                    }

                    // Atividades Secundarias -> CnaesSecundarios
                    if (fornecedorWs.AtividadesSecundarias != null)
                    {
                        info.CnaesSecundarios = fornecedorWs.AtividadesSecundarias.Select(a => new MinhaReceita.FornecedorAtividade
                        {
                            Id = ParseCodeToInt(a.Code),
                            Codigo = a.Code,
                            Descricao = a.Text
                        }).ToList();
                    }

                    // Natureza juridica
                    info.NaturezaJuridica = fornecedorWs.NaturezaJuridica;
                    info.IdNaturezaJuridica = ParseCodeToInt(fornecedorWs.NaturezaJuridica);

                    // QSA
                    if (fornecedorWs.Qsa != null)
                    {
                        info.Qsa = fornecedorWs.Qsa.Select(q => new MinhaReceita.QuadroSocietario
                        {
                            Nome = q.Nome,
                            PaisOrigem = q.PaisOrigem,
                            NomeRepresentante = q.NomeRepLegal,
                            QualificacaoSocio = q.Qual,
                            QualificacaoRepresentante = q.QualRepLegal
                        }).ToList();
                    }

                    return info;
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine(response.ReasonPhrase);
                        System.Threading.Thread.Sleep(60000);

                        return await ConsultaCnpjReceitaWs(cnpj);
                    }
                    else
                    {
                        InserirControle(1, cnpj, response.ReasonPhrase.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                InserirControle(1, cnpj, ex.GetBaseException().Message);
            }

            return null;
        }

        private void InserirControle(int controle, string cnpj_cpf, string mensagem)
        {
            using (var context = dbContextFactory.CreateDbContext())
            {
                context.Database.ExecuteSqlRaw(@"update fornecedor set controle=@controle, mensagem=@mensagem where cnpj_cpf=@cnpj_cpf;",
                    controle, mensagem, cnpj_cpf);
            }

            if (controle > 0)
                logger.LogInformation("Controle Fornecedor: {Controle} - {CnpjCpf} - {Mensagem}", controle, cnpj_cpf, mensagem);
        }

        private uint LocalizaInsereAtividade(AppDbContext context, List<FornecedorAtividade> lstFornecedoresAtividade, int codigo, string descricao)
        {
            var codigoFormatado = codigo.ToString(@"00\.00-0-00");
            var item = lstFornecedoresAtividade.FirstOrDefault(x => x.Codigo == codigoFormatado);
            if (item == null)
            {
                var atividade = new FornecedorAtividade()
                {
                    Codigo = codigoFormatado,
                    Descricao = descricao
                };
                context.FornecedorAtividades.Add(atividade);
                context.SaveChanges();

                lstFornecedoresAtividade.Add(atividade);

                return atividade.Id;
            }

            return item.Id;
        }

        private int LocalizaInsereNaturezaJuridica(AppDbContext context, List<FornecedorNaturezaJuridica> lstNaturezaJuridica, string codigo, string descricao)
        {
            var item = lstNaturezaJuridica.FirstOrDefault(x => x.Codigo == codigo);
            if (item == null)
            {

                var atividade = new FornecedorNaturezaJuridica()
                {
                    Codigo = codigo,
                    Descricao = descricao
                };
                context.FornecedorNaturezaJuridicas.Add(atividade);
                context.SaveChanges();

                lstNaturezaJuridica.Add(atividade);
                return atividade.Id;
            }

            return item.Id;
        }

        public static bool validarCNPJ(string cnpj)
        {
            int[] mt1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mt2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma; int resto; string digito; string TempCNPJ;

            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14)
                return false;

            if (cnpj == "00000000000000" || cnpj == "11111111111111" ||
             cnpj == "22222222222222" || cnpj == "33333333333333" ||
             cnpj == "44444444444444" || cnpj == "55555555555555" ||
             cnpj == "66666666666666" || cnpj == "77777777777777" ||
             cnpj == "88888888888888" || cnpj == "99999999999999")
                return false;

            TempCNPJ = cnpj.Substring(0, 12);
            soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(TempCNPJ[i].ToString()) * mt1[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            TempCNPJ = TempCNPJ + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(TempCNPJ[i].ToString()) * mt2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

        //public void AtualizaFornecedorDoador()
        //{
        //    using (var banco = new AppDb())
        //    {
        //        var dt = banco.GetTable("select id, cnpj_cpf from fornecedor");

        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            banco.AddParameter("cnpj", dr["cnpj_cpf"]);
        //            var existe = banco.ExecuteScalar("select 1 from eleicao_doacao where raiz_cnpj_cpf_doador=@cnpj;");
        //            if (existe != null)
        //            {
        //                banco.AddParameter("id", dr["id"]);
        //                banco.ExecuteNonQuery("update fornecedor set doador=1 where id=@id");
        //            }
        //        }
        //    }
        //}
    }
}
