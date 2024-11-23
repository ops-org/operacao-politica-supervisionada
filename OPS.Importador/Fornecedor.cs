using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;

namespace OPS.Importador
{
    public class Fornecedor
    {
        public ILogger<Fornecedor> logger { get; private set; }

        private readonly IDbConnection connection;

        public IConfiguration configuration { get; private set; }

        public HttpClient httpClient { get; }

        public Fornecedor(ILogger<Fornecedor> logger, IDbConnection connection, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.connection = connection;
            this.configuration = configuration;

            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");
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

        public async Task ConsultarDadosCNPJ()
        {
            var telegramApiToken = configuration["AppSettings:TelegramApiToken"];
            var receitaWsApiToken = configuration["AppSettings:ReceitaWsApiToken"];

            var telegram = new TelegramApi(telegramApiToken);
            var telegraMessage = new Core.Entity.TelegramMessage()
            {
                ChatId = "-1001378778982", // OPS - Alertas
                ParseMode = "html",
                Text = ""
            };

            int totalImportados = 0;

            DataTable dtFornecedores;
            List<FornecedorAtividade> lstFornecedoresAtividade;
            List<NaturezaJuridica> lstNaturezaJuridica;

            using (var banco = new AppDb())
            {
                dtFornecedores = banco.GetTable(
                    @"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                    from fornecedor f
                    left join fornecedor_info fi on f.id = fi.id_fornecedor
                    where char_length(f.cnpj_cpf) = 14
                    -- and f.cnpj_cpf <> '00000000000000'
                    -- and obtido_em < '2018-01-01'
                    -- and fi.id_fornecedor is null
                    -- and ip_colaborador not like '1805%'
                    -- and fi.id_fornecedor is null
                    -- and ip_colaborador is null -- not in ('170509', '170510', '170511', '170512')
                    -- and controle is null
                    -- and controle <> 0
                    -- and (f.mensagem is null or f.mensagem <> 'Uma tarefa foi cancelada.')
                    -- and (controle is null or controle NOT IN (0, 2, 3, 5))
                    -- and fi.situacao_cadastral = 'ATIVA'
                    and (controle is null or controle NOT IN (0, 1, 2, 3))
                    -- AND (fi.situacao_cadastral is null or fi.situacao_cadastral = 'ATIVA')
                    AND fi.id_fornecedor IS null
                    AND f.cnpj_cpf NOT LIKE '%*%' -- Dados anonimizados
                    order by fi.id_fornecedor asc
                    -- LIMIT 1000");

                //dtFornecedores = banco.GetTable(
                //   $@"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                //    from fornecedor f
                //    left join fornecedor_info fi on f.id = fi.id_fornecedor
                //    where char_length(f.cnpj_cpf) = 14
                //    and obtido_em < '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd")}'
                //    and fi.situacao_cadastral = 'ATIVA'
                //    order by fi.id_fornecedor asc");

                if (dtFornecedores.Rows.Count == 0)
                {
                    logger.LogInformation("Não há fornecedores para consultar");
                    return;
                }


                lstFornecedoresAtividade = connection.GetList<FornecedorAtividade>().ToList();
                lstNaturezaJuridica = connection.GetList<NaturezaJuridica>().ToList();
            }

            logger.LogInformation("Consultando CNPJ's Local: {Total} itens.", dtFornecedores.Rows.Count);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int atual = 0;
            int total = dtFornecedores.Rows.Count;
            foreach (DataRow item in dtFornecedores.Rows)
            {
                // the code that you want to measure comes here
                //watch.Stop();
                //_logger.LogInformation(watch.ElapsedMilliseconds);
                //watch.Restart();

                atual++;
                if (!validarCNPJ(item["cnpj_cpf"].ToString()))
                {
                    InserirControle(3, item["cnpj_cpf"].ToString(), "CNPJ Invalido");
                    logger.LogInformation("CNPJ Invalido [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, total, item["cnpj_cpf"].ToString(), item["nome"]);

                    //telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.net.br/fornecedor/{item["id"]}'>{item["cnpj_cpf"]} - {item["nome"]}</a>; Motivo: CNPJ Invalido";
                    //telegram.SendMessage(telegraMessage);
                    continue;
                }

                //_logger.LogInformation("Consultando CNPJ: " + item["cnpj_cpf"] + " - " + i);
                FornecedorInfo fornecedor = null;

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"https://minhareceita.org/{item["cnpj_cpf"]}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        fornecedor = JsonSerializer.Deserialize<FornecedorInfo>(responseString);
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
                            InserirControle(1, item["cnpj_cpf"].ToString(), response.ReasonPhrase.ToString());
                        }

                        continue;
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    InserirControle(1, item["cnpj_cpf"].ToString(), ex.GetBaseException().Message);
                    continue;
                }

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                try
                {
                    fornecedor.Id = Convert.ToInt32(item["id"]);
                    fornecedor.ObtidoEm = DateTime.Today;

                    fornecedor.RadicalCnpj = fornecedor.Cnpj.Substring(0, 8); //13.106.352/0001-78

                    if (item["id_fornecedor"] != DBNull.Value)
                    {
                        connection.Execute(@"
delete from fornecedor_info where id_fornecedor=@id;
delete from fornecedor_atividade_secundaria where id_fornecedor=@id;
delete from fornecedor_socio where id_fornecedor=@id;
                            ", new { id = fornecedor.Id }, transaction);
                    }

                    if (fornecedor.IdAtividadePrincipal > 0)
                    {
                        fornecedor.IdAtividadePrincipal = LocalizaInsereAtividade(transaction, lstFornecedoresAtividade, fornecedor.IdAtividadePrincipal, fornecedor.AtividadePrincipal);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    if (fornecedor.IdNaturezaJuridica > 0)
                    {
                        fornecedor.IdNaturezaJuridica = LocalizaInsereNaturezaJuridica(transaction, lstNaturezaJuridica, fornecedor.IdNaturezaJuridica, fornecedor.NaturezaJuridica);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    if (fornecedor.MotivoSituacaoCadastral == "SEM MOTIVO")
                        fornecedor.MotivoSituacaoCadastral = null;

                    connection.Insert(fornecedor, transaction);

                    connection.Execute(@"update fornecedor set nome=@nome where id=@id", new
                    {
                        id = fornecedor.Id,
                        nome = fornecedor.RazaoSocial
                    }, transaction);


                    foreach (var atividadesSecundaria in fornecedor.CnaesSecundarios)
                    {
                        try
                        {
                            var idAtividade = LocalizaInsereAtividade(transaction, lstFornecedoresAtividade, atividadesSecundaria.Id, atividadesSecundaria.Descricao);

                            var atividade = new FornecedorAtividadeSecundaria();
                            atividade.IdFornecedor = fornecedor.Id;
                            atividade.IdAtividade = idAtividade;

                            connection.Insert(atividade, transaction);
                        }
                        catch (MySqlException ex)
                        {
                            if (!ex.Message.Contains("Duplicate entry")) throw;
                        }
                    }

                    if (fornecedor.Qsa != null)
                        foreach (var qsa in fornecedor.Qsa)
                        {
                            qsa.IdFornecedor = fornecedor.Id;

                            if (qsa.CpfRepresentante == "***000000**")
                                qsa.CpfRepresentante = null;

                            connection.Insert(qsa, transaction);
                        }

                    totalImportados++;

                    if (fornecedor.SituacaoCadastral != "ATIVA")
                    {
                        if (fornecedor.DataSituacaoEspecial != null)
                        {
                            var ulimaNota = connection.ExecuteScalar(@"
SELECT MAX(DATA) as data FROM (
	SELECT MAX(d.data_emissao) AS data FROM cf_despesa d WHERE d.id_fornecedor = @id
	UNION ALL
	SELECT MAX(d.data_emissao) FROM sf_despesa d WHERE d.id_fornecedor = @id
	UNION ALL
	SELECT MAX(d.data_emissao) FROM cl_despesa d WHERE d.id_fornecedor = @id
) tmp
", new { id = fornecedor.Id }, transaction);
                            if (ulimaNota != null && Convert.ToDateTime(ulimaNota) > Convert.ToDateTime(fornecedor.DataSituacaoEspecial))
                            {
                                logger.LogInformation("Empresa Inativa [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, total, fornecedor.Cnpj, fornecedor.NomeFantasia);

                                telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.net.br/fornecedor/{item["id"]}'>{fornecedor.Cnpj} - {fornecedor.NomeFantasia}</a>";
                                telegram.SendMessage(telegraMessage);
                            }
                        }
                    }

                    transaction.Commit();

                    InserirControle(0, item["cnpj_cpf"].ToString(), "");
                    logger.LogInformation("Atualizando [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", atual, total, fornecedor.Cnpj, fornecedor.NomeFantasia);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }

                //    else
                //{
                //    InserirControle(2, item["cnpj_cpf"].ToString(), receita.message);
                //    _logger.LogInformation("Empresa {Status} [{Atual}/{Total}] {CNPJ} {NomeEmpresa}", receita.message, atual, total, receita.cnpj, receita.nome);

                //    //telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.net.br/fornecedor/{item["id"]}'>{receita.cnpj} - {receita.nome}</a>; Motivo: {receita.message}";
                //    //telegram.SendMessage(telegraMessage);
                //}
            }

            //        using (var banco = new AppDb())
            //        {
            //            banco.ExecuteNonQuery(@"
            //	update fornecedor_info set nome_fantasia=null where nome_fantasia = '' or nome_fantasia = '********';
            //	update fornecedor_info set logradouro=null where logradouro = '' or logradouro = '********';
            //	update fornecedor_info set numero=null where numero = '' or numero = '********';
            //	update fornecedor_info set complemento=null where complemento = '' or complemento = '********';
            //	update fornecedor_info set cep=null where cep = '' or cep = '********';
            //	update fornecedor_info set bairro=null where bairro = '' or bairro = '********';
            //	update fornecedor_info set municipio=null where municipio = '' or municipio = '********';
            //	update fornecedor_info set estado=null where estado = '' or estado = '**';
            //	update fornecedor_info set endereco_eletronico=null where endereco_eletronico = '' or endereco_eletronico = '********';
            //	update fornecedor_info set telefone=null where telefone = '' or telefone = '********';
            //	update fornecedor_info set ente_federativo_responsavel=null where ente_federativo_responsavel = '' or ente_federativo_responsavel = '********';
            //	update fornecedor_info set motivo_situacao_cadastral=null where motivo_situacao_cadastral = '' or motivo_situacao_cadastral = '********';
            //	update fornecedor_info set situacao_especial=null where situacao_especial = '' or situacao_especial = '********';
            //");
            //        }

            //_logger.LogInformation("{0} de {1} fornecedores novos importados</p>", totalImportados, dtFornecedores.Rows.Count) + strInfoAdicional.ToString();
        }

        private void InserirControle(int controle, string cnpj_cpf, string mensagem)
        {
            using (var banco = new AppDb())
            {
                banco.AddParameter("@cnpj_cpf", cnpj_cpf);
                banco.AddParameter("@controle", controle);
                banco.AddParameter("@mensagem", mensagem);

                banco.ExecuteNonQuery(@"update fornecedor set controle=@controle, mensagem=@mensagem where cnpj_cpf=@cnpj_cpf;");
            }

            if (controle > 0)
                logger.LogInformation("Controle Fornecedor: {Controle} - {CnpjCpf} - {Mensagem}", controle, cnpj_cpf, mensagem);
        }

        private int LocalizaInsereAtividade(IDbTransaction transaction, List<FornecedorAtividade> lstFornecedoresAtividade, int codigo, string descricao)
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
                atividade.Id = (int)connection.Insert(atividade, transaction);

                lstFornecedoresAtividade.Add(atividade);
                return atividade.Id;
            }

            return item.Id;
        }

        private int LocalizaInsereNaturezaJuridica(IDbTransaction transaction, List<NaturezaJuridica> lstFornecedoresAtividade, int codigo, string descricao)
        {
            var codigoFormatado = codigo.ToString("000-0");
            var item = lstFornecedoresAtividade.FirstOrDefault(x => x.Codigo == codigoFormatado);
            if (item == null)
            {
                var atividade = new NaturezaJuridica()
                {
                    Codigo = codigoFormatado,
                    Descricao = descricao
                };
                atividade.Id = (int)connection.Insert(atividade, transaction);

                lstFornecedoresAtividade.Add(atividade);
                return atividade.Id;
            }

            return item.Id;
        }

        private object ObterValor(object d)
        {
            if (Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()))
                return DBNull.Value;
            try
            {
                var sValor = d.ToString().Split(' ');

                if (sValor.Length == 1)
                {
                    return Convert.ToDecimal(sValor[0], new CultureInfo("en-US"));
                }
                else
                {
                    return Convert.ToDecimal(sValor[1], new CultureInfo("en-US"));
                }
            }
            catch
            {
                return DBNull.Value;
            }
        }

        private object ParseDate(object d)
        {
            if (Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()) || (d.ToString() == "0000-00-00 00:00:00") ||
                d.ToString().StartsWith("*"))
                return DBNull.Value;
            try
            {
                return Convert.ToDateTime(d);
            }
            catch
            {
                return DBNull.Value;
            }
        }

        public bool validarCNPJ(string cnpj)
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
    }
}