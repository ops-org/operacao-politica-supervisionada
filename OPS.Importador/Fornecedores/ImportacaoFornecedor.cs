using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OPS.Core.DTOs;
using OPS.Core.Utilities;
using OPS.Importador.Comum;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Importador.Fornecedores
{
    public class FornecedorQueryResult
    {
        public string CnpjCpf { get; set; }
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public class ImportacaoFornecedor
    {
        private readonly ILogger<ImportacaoFornecedor> logger;
        private readonly AppSettings appSettings;
        private readonly AppDbContext dbContext;
        private readonly HttpClient httpClient;

        public ImportacaoFornecedor(ILogger<ImportacaoFornecedor> logger,IServiceProvider serviceProvider, AppDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;

            appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");
        }

        public async Task ConsultarDadosCNPJ(bool somenteNovos = true)
        {
            var telegram = new TelegramApi(appSettings.TelegramApiToken);
            var telegraMessage = new TelegramMessage()
            {
                ChatId = "-1001378778982", // OPS - Alertas
                ParseMode = "html",
                Text = ""
            };

            //using (var context = dbContextFactory.CreateDbContext())
            {
                var fornecedores = await ObterFornecedoresParaConsulta(somenteNovos);
                if (fornecedores.Count == 0)
                {
                    logger.LogInformation("Não há fornecedores para consultar");
                    return;
                }

                logger.LogInformation("Consultando CNPJ's Local: {Total} itens.", fornecedores.Count);

                int totalImportados = await ProcessarFornecedores(fornecedores, telegram, telegraMessage);
                logger.LogInformation("{TotalImportados} de {TotalFornecedores} fornecedores processados", totalImportados, fornecedores.Count);

                // Atualizar dados anonimizados para NULL (ReceitaWS)
                await dbContext.Database.ExecuteSqlRawAsync(@"
                	update fornecedor.fornecedor_info set nome_fantasia=null where nome_fantasia = '' or nome_fantasia = '********';
                	update fornecedor.fornecedor_info set logradouro=null where logradouro = '' or logradouro = '********';
                	update fornecedor.fornecedor_info set numero=null where numero = '' or numero = '********';
                	update fornecedor.fornecedor_info set complemento=null where complemento = '' or complemento = '********';
                	update fornecedor.fornecedor_info set cep=null where cep = '' or cep = '********';
                	update fornecedor.fornecedor_info set bairro=null where bairro = '' or bairro = '********';
                	update fornecedor.fornecedor_info set municipio=null where municipio = '' or municipio = '********';
                	update fornecedor.fornecedor_info set estado=null where estado = '' or estado = '**';
                	update fornecedor.fornecedor_info set endereco_eletronico=null where endereco_eletronico = '' or endereco_eletronico = '********';
                	update fornecedor.fornecedor_info set telefone1=null where telefone1 = '' or telefone1 = '********';
                	update fornecedor.fornecedor_info set telefone2=null where telefone2 = '' or telefone2 = '********';
                	update fornecedor.fornecedor_info set ente_federativo_responsavel=null where ente_federativo_responsavel = '' or ente_federativo_responsavel = '********';
                	update fornecedor.fornecedor_info set motivo_situacao_cadastral=null where motivo_situacao_cadastral = '' or motivo_situacao_cadastral = '********';
                	update fornecedor.fornecedor_info set situacao_especial=null where situacao_especial = '' or situacao_especial = '********';
                ");
            }
        }

        private async Task<List<FornecedorQueryResult>> ObterFornecedoresParaConsulta(bool somenteNovos)
        {
            if (somenteNovos)
            {
                return await dbContext.Fornecedores.FromSqlRaw(
                    @"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                    from fornecedor.fornecedor f
                    left join fornecedor.fornecedor_info fi on f.id = fi.id_fornecedor
                    where char_length(f.cnpj_cpf) = 14
                    and (controle is null or controle NOT IN (0, 1, 2, 3))
                    AND fi.id_fornecedor IS null
                    AND f.cnpj_cpf NOT ILIKE '%*%' -- Dados anonimizados
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
                return await dbContext.Fornecedores.FromSqlInterpolated(
                    $@"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                    from fornecedor.fornecedor f
                    left join fornecedor.fornecedor_info fi on f.id = fi.id_fornecedor
                    where char_length(f.cnpj_cpf) = 14
                    and obtido_em < {cutoffDate}
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

        private async Task<int> ProcessarFornecedores(List<FornecedorQueryResult> fornecedores, TelegramApi telegram, TelegramMessage telegraMessage)
        {
            int totalImportados = 0;
            int atual = 0;

            var lstFornecedoresAtividade = await dbContext.FornecedorAtividades.ToListAsync();
            var lstNaturezaJuridica = await dbContext.FornecedorNaturezaJuridicas.ToListAsync();

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
                using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    fornecedor.Id = item.Id;
                    fornecedor.ObtidoEm = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
                    fornecedor.RadicalCnpj = fornecedor.Cnpj.Substring(0, 8);

                    await dbContext.FornecedorSocios.AsNoTracking().Where(x => x.IdFornecedor == fornecedor.Id).ExecuteDeleteAsync();
                    await dbContext.FornecedorAtividadesSecundarias.AsNoTracking().Where(x => x.IdFornecedor == fornecedor.Id).ExecuteDeleteAsync();

                    if (fornecedor.IdAtividadePrincipal > 0)
                        fornecedor.IdAtividadePrincipal = await LocalizaInsereAtividade(lstFornecedoresAtividade, fornecedor.IdAtividadePrincipal, fornecedor.AtividadePrincipal);

                    if (fornecedor.IdNaturezaJuridica > 0)
                        fornecedor.IdNaturezaJuridica = await LocalizaInsereNaturezaJuridica(lstNaturezaJuridica, fornecedor.IdNaturezaJuridica.ToString("000-0"), fornecedor.NaturezaJuridica);

                    if (fornecedor.MotivoSituacaoCadastral == "SEM MOTIVO")
                        fornecedor.MotivoSituacaoCadastral = null;

                    var fornecedorInfo = await dbContext.FornecedorInfos.FirstOrDefaultAsync(f => f.IdFornecedor == fornecedor.Id);

                    if (fornecedorInfo == null)
                    {
                        fornecedorInfo = new FornecedorInfo()
                        {
                            IdFornecedor = fornecedor.Id
                        };

                        dbContext.FornecedorInfos.Add(fornecedorInfo);
                    }

                    fornecedorInfo.Cnpj = fornecedor.Cnpj;
                    fornecedorInfo.CnpjRadical = fornecedor.RadicalCnpj;
                    fornecedorInfo.Tipo = fornecedor.Tipo;
                    fornecedorInfo.Nome = fornecedor.RazaoSocial;
                    fornecedorInfo.DataDeAbertura = fornecedor.Abertura;
                    fornecedorInfo.NomeFantasia = fornecedor.NomeFantasia;
                    fornecedorInfo.IdFornecedorAtividadePrincipal = fornecedor.IdAtividadePrincipal;
                    fornecedorInfo.IdFornecedorNaturezaJuridica = fornecedor.IdNaturezaJuridica;
                    fornecedorInfo.LogradouroTipo = fornecedor.TipoLogradouro;
                    fornecedorInfo.Logradouro = fornecedor.Logradouro;
                    fornecedorInfo.Numero = fornecedor.Numero;
                    fornecedorInfo.Complemento = fornecedor.Complemento;
                    fornecedorInfo.Cep = fornecedor.Cep;
                    fornecedorInfo.Bairro = fornecedor.Bairro;
                    fornecedorInfo.Municipio = fornecedor.Municipio;
                    fornecedorInfo.Estado = fornecedor.UF;
                    fornecedorInfo.EnderecoEletronico = fornecedor.Email;
                    fornecedorInfo.Telefone1 = fornecedor.Telefone1;
                    fornecedorInfo.Fax = fornecedor.DddFax;
                    fornecedorInfo.EnteFederativoResponsavel = fornecedor.EnteFederativoResponsavel;
                    fornecedorInfo.SituacaoCadastral = fornecedor.SituacaoCadastral;
                    fornecedorInfo.DataDaSituacaoCadastral = fornecedor.DataSituacaoCadastral;
                    fornecedorInfo.MotivoSituacaoCadastral = fornecedor.MotivoSituacaoCadastral;
                    fornecedorInfo.SituacaoEspecial = fornecedor.SituacaoEspecial;
                    fornecedorInfo.DataSituacaoEspecial = fornecedor.DataSituacaoEspecial;
                    fornecedorInfo.CapitalSocial = fornecedor.CapitalSocial;
                    fornecedorInfo.Porte = fornecedor.Porte;
                    fornecedorInfo.OpcaoPeloMei = fornecedor.OpcaoPeloMEI;
                    fornecedorInfo.DataOpcaoPeloMei = fornecedor.DataOpcaoPeloMEI;
                    fornecedorInfo.DataExclusaoDoMei = fornecedor.DataExclusaoMEI;
                    fornecedorInfo.OpcaoPeloSimples = fornecedor.OpcaoPeloSimples;
                    fornecedorInfo.DataOpcaoPeloSimples = fornecedor.DataOpcaoPeloSimples;
                    fornecedorInfo.DataExclusaoDoSimples = fornecedor.DataExclusaoSimples;
                    fornecedorInfo.CodigoMunicipioIbge = fornecedor.CodigoMunicipioIBGE?.ToString();
                    fornecedorInfo.NomeCidadeNoExterior = fornecedor.NomeCidadeExterior;
                    fornecedorInfo.ObtidoEm = fornecedor.ObtidoEm;
                    fornecedorInfo.NomePais = fornecedor.Pais;

                    dbContext.Fornecedores.Where(x => x.Id == fornecedor.Id)
                        .ExecuteUpdate(x => x.SetProperty(y => y.Nome, fornecedor.RazaoSocial).SetProperty(y => y.Categoria, "PJ"));

                    // Process secondary activities
                    if (fornecedor.CnaesSecundarios != null)
                    {
                        foreach (var atividadesSecundaria in fornecedor.CnaesSecundarios.DistinctBy(x => x.Id))
                        {
                            var idAtividade = await LocalizaInsereAtividade(lstFornecedoresAtividade, atividadesSecundaria.Id, atividadesSecundaria.Descricao);

                            var atividade = new FornecedorAtividadeSecundaria()
                            {
                                IdFornecedor = (int)fornecedor.Id,
                                IdAtividade = idAtividade
                            };

                            dbContext.FornecedorAtividadesSecundarias.Add(atividade);
                        }

                    }

                    // Process partners (QSA)
                    if (fornecedor.Qsa != null)
                    {
                        var socios = fornecedor.Qsa.Select(qsa =>
                        {
                            qsa.IdFornecedor = fornecedor.Id;
                            if (qsa.CpfRepresentante == "***000000**")
                                qsa.CpfRepresentante = null;

                            return new FornecedorSocio
                            {
                                IdFornecedor = (int)qsa.IdFornecedor,
                                CnpjCpf = qsa.CnpjCpf,
                                Nome = qsa.Nome,
                                PaisOrigem = qsa.PaisOrigem,
                                NomeRepresentante = qsa.NomeRepresentante,
                                CpfRepresentante = qsa.CpfRepresentante,
                                IdFornecedorSocioQualificacao = qsa.IdSocioQualificacao > 0 ? (byte?)qsa.IdSocioQualificacao : null,
                                IdFornecedorSocioRepresentanteQualificacao = qsa.IdSocioRepresentanteQualificacao > 0 ? (byte?)qsa.IdSocioRepresentanteQualificacao : null,
                                IdFornecedorFaixaEtaria = qsa.IdFaixaEtaria,
                                DataEntradaSociedade = !string.IsNullOrEmpty(qsa.DataEntradaSociedade) ? DateTime.Parse(qsa.DataEntradaSociedade).Date : null
                            };
                        }).ToList();

                        dbContext.FornecedorSocios.AddRange(socios);
                        //await context.SaveChangesAsync();
                    }

                    totalImportados++;

                    // Check for inactive companies with recent activity
                    if (fornecedor.SituacaoCadastral != "ATIVA" && fornecedor.DataSituacaoEspecial != null)
                    {
                        var connection = dbContext.Database.GetDbConnection();
                        var command = connection.CreateCommand();
                        command.CommandText = $@"
                            SELECT MAX(data) FROM (
                                SELECT MAX(d.data_emissao) as data FROM camara.cf_despesa d WHERE d.id_fornecedor = {fornecedor.Id}
                                UNION ALL
                                SELECT MAX(d.data_emissao) as data FROM senado.sf_despesa d WHERE d.id_fornecedor = {fornecedor.Id}
                                UNION ALL
                                SELECT MAX(d.data_emissao) as data FROM assembleias.cl_despesa d WHERE d.id_fornecedor = {fornecedor.Id}
                            ) tmp";
                        var ultimaNota = await command.ExecuteScalarAsync() as DateOnly?;

                        if (ultimaNota != null && fornecedor.DataSituacaoEspecial.HasValue && ultimaNota > DateOnly.FromDateTime(fornecedor.DataSituacaoEspecial.Value))
                        {
                            logger.LogInformation("Empresa Inativa [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, fornecedores.Count, item.CnpjCpf, item.Nome);

                            telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.org.br/fornecedor/{item.Id}'>{item.CnpjCpf} - {item.Nome}</a>";
                            telegram.SendMessage(telegraMessage);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    dbContext.ChangeTracker.Clear();
                    InserirControle(0, item.CnpjCpf, "");
                    logger.LogInformation("Atualizando [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, fornecedores.Count, item.CnpjCpf, item.Nome);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Erro ao processar fornecedor {CNPJ}", item.CnpjCpf);
                    InserirControle(1, item.CnpjCpf, ex.Message);
                }
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
                    var fornecedorWs = JsonSerializer.Deserialize<ReceitaWS.FornecedorReceitaWs>(responseString);

                    if (fornecedorWs == null || fornecedorWs.Status == "ERROR")
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
                        DataOpcaoPeloMEI = fornecedorWs.Simei is null ? null : ParseDateString(fornecedorWs.Simei.DataOpcao),
                        DataExclusaoMEI = fornecedorWs.Simei is null ? null : ParseDateString(fornecedorWs.Simei.DataExclusao),
                        OpcaoPeloSimples = fornecedorWs.Simples?.Optante,
                        DataOpcaoPeloSimples = fornecedorWs.Simples is null ? null : ParseDateString(fornecedorWs.Simples.DataOpcao),
                        DataExclusaoSimples = fornecedorWs.Simples is null ? null : ParseDateString(fornecedorWs.Simples.DataExclusao),
                        NomeCidadeExterior = null,
                        Pais = null,
                        ObtidoEm = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified)
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
            //using (var context = dbContextFactory.CreateDbContext())
            {
                dbContext.Fornecedores
                    .Where(x => x.CnpjCpf == cnpj_cpf)
                        .ExecuteUpdate(x => x
                            .SetProperty(y => y.Controle, (sbyte)controle)
                            .SetProperty(y => y.Mensagem, mensagem));
            }

            if (controle > 0)
                logger.LogInformation("Controle Fornecedor: {Controle} - {CnpjCpf} - {Mensagem}", controle, cnpj_cpf, mensagem);
        }

        private async Task<int> LocalizaInsereAtividade(List<FornecedorAtividade> lstFornecedoresAtividade, int codigo, string descricao)
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
                dbContext.FornecedorAtividades.Add(atividade);
                //await dbContext.SaveChangesAsync();

                lstFornecedoresAtividade.Add(atividade);

                // Don't save here - let the main transaction handle it
                return atividade.Id;
            }

            return item.Id;
        }

        private async Task<int> LocalizaInsereNaturezaJuridica(List<FornecedorNaturezaJuridica> lstNaturezaJuridica, string codigo, string descricao)
        {
            var item = lstNaturezaJuridica.FirstOrDefault(x => x.Codigo == codigo);
            if (item == null)
            {

                var atividade = new FornecedorNaturezaJuridica()
                {
                    Codigo = codigo,
                    Descricao = descricao
                };
                dbContext.FornecedorNaturezaJuridicas.Add(atividade);
                //await dbContext.SaveChangesAsync();

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
