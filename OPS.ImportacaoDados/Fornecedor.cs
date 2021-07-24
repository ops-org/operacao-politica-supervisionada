using MySqlConnector;
using Newtonsoft.Json;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OPS.ImportacaoDados
{
    public static class Fornecedor
    {
        //public static void ConsultarCNPJ()
        //{
        //	int totalErros = 0, totalAcertos = 0;

        //	while (true)
        //	{
        //		DataTable dtFornecedores;
        //		using (var banco = new Banco())
        //		{
        //			dtFornecedores = banco.GetTable(
        //				@"select cnpj_cpf, f.id, fi.id_fornecedor
        //                  from fornecedor f
        //                  left join fornecedor_info fi on f.id = fi.id_fornecedor
        //                  where char_length(f.cnpj_cpf) = 14
        //                  and f.cnpj_cpf <> '00000000000000'
        //                  -- and obtido_em < '2017-01-01'
        //                  and fi.id_fornecedor is null
        //                  -- and ip_colaborador is null -- not in ('170509', '170510', '170511', '170512')
        //                  -- and controle is null
        //                  -- and controle = 1
        //			and controle <> 5
        //                  -- and (f.mensagem is null or f.mensagem <> 'O número do CNPJ não foi localizado na Receita Federal')
        //                  order by 1 desc");
        //		}

        //		if (dtFornecedores.Rows.Count == 0) break;

        //		Console.WriteLine("Consultando CNPJ's Local: {0} itens.", dtFornecedores.Rows.Count);

        //		foreach (DataRow item in dtFornecedores.Rows)
        //		{
        //			try
        //			{
        //				Receita.ConsultarCNPJ(item["cnpj_cpf"].ToString(), ref totalAcertos, ref totalErros);
        //			}
        //			catch (Exception e)
        //			{
        //				Console.WriteLine(e.Message);
        //				System.Threading.Thread.Sleep(3600000);
        //			}
        //		}
        //	};
        //}

        public static void AtualizaFornecedorDoador()
        {
            using (var banco = new AppDb())
            {
                var dt = banco.GetTable("select id, cnpj_cpf from fornecedor");

                foreach (DataRow dr in dt.Rows)
                {
                    banco.AddParameter("cnpj", dr["cnpj_cpf"]);
                    var existe = banco.ExecuteScalar("select 1 from eleicao_doacao where raiz_cnpj_cpf_doador=@cnpj;");
                    if (existe != null)
                    {
                        banco.AddParameter("id", dr["id"]);
                        banco.ExecuteNonQuery("update fornecedor set doador=1 where id=@id");
                    }
                }
            }
        }

        public static async Task<string> ConsultarReceitaWS(string receitaWsApiToken, string telegramApiToken)
        {
            var telegram = new TelegramApi(telegramApiToken);
            var telegraMessage = new Core.Entity.TelegramMessage()
            {
                ChatId = "-1001378778982", // OPS - Alertas
                ParseMode = "html",
                Text = ""
            };

            var sb = new StringBuilder();
            var strInfoAdicional = new StringBuilder();
            int RateLimit_Remaining = 3;
            int totalImportados = 0;

            DataTable dtFornecedores;
            DataTable dtFornecedoresAtividade;
            DataTable dtFornecedoresNatJu;

            using (var banco = new AppDb())
            {
                dtFornecedores = banco.GetTable(
                    @"select cnpj_cpf, f.id, fi.id_fornecedor, f.nome
                    from fornecedor f
                    left join fornecedor_info fi on f.id = fi.id_fornecedor
                    where char_length(f.cnpj_cpf) = 14
                    and f.cnpj_cpf <> '00000000000000'
                    -- and obtido_em < '2018-01-01'
                    -- and fi.id_fornecedor is null
                    -- and ip_colaborador not like '1805%'
                    -- and fi.id_fornecedor is null
                    -- and ip_colaborador is null -- not in ('170509', '170510', '170511', '170512')
                    -- and controle is null
                    -- and controle <> 0
					-- and (f.mensagem is null or f.mensagem <> 'Uma tarefa foi cancelada.')
					-- and (controle is null or controle NOT IN (0, 2, 3, 5))
                    and (controle is null or controle NOT IN (0, 3))
                    AND (fi.situacao_cadastral is null or fi.situacao_cadastral = 'ATIVA')
                    order by fi.obtido_em asc
                    LIMIT 2, 10000");

                if (dtFornecedores.Rows.Count == 0)
                {
                    Console.WriteLine("Não há fornecedores para consultar");
                    return "<p>Não há fornecedores para consultar</p>";
                }

                dtFornecedoresAtividade = banco.GetTable("SELECT * FROM fornecedor_atividade;");
                dtFornecedoresNatJu = banco.GetTable("SELECT * FROM fornecedor_natureza_juridica;");
            }

            Console.WriteLine("Consultando CNPJ's Local: {0} itens.", dtFornecedores.Rows.Count);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int i = 0;
            foreach (DataRow item in dtFornecedores.Rows)
            {
                // the code that you want to measure comes here
                //watch.Stop();
                //Console.WriteLine(watch.ElapsedMilliseconds);
                //watch.Restart();

                i++;
                if (!validarCNPJ(item["cnpj_cpf"].ToString()))
                {
                    InserirControle(3, item["cnpj_cpf"].ToString(), "CNPJ Invalido");
                    Console.WriteLine("CNPJ Invalido: " + item["cnpj_cpf"] + " - " + i);

                    strInfoAdicional.Append("<p>Empresa invalida importada:" + item["id"].ToString() + " - " + item["cnpj_cpf"].ToString() + " - " + item["nome"].ToString() + "; Motivo: CNPJ Invalido</p>");

                    telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.net.br/fornecedor/{item["id"]}'>{item["cnpj_cpf"]} - {item["nome"]}</a>; Motivo: CNPJ Invalido";
                    telegram.SendMessage(telegraMessage);
                    continue;
                }

                //Console.WriteLine("Consultando CNPJ: " + item["cnpj_cpf"] + " - " + i);
                FornecedorInfo receita = null;

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", receitaWsApiToken);

                        //--------------------------------
                        string uriString;

                        uriString = string.Format("https://www.receitaws.com.br/v1/cnpj/{0}", item["cnpj_cpf"].ToString());
                        client.BaseAddress = new Uri(uriString);

                        if (RateLimit_Remaining == 0)
                        {
                            watch.Stop();
                            Console.WriteLine("Rate limit atingido! " + (60 - watch.Elapsed.TotalSeconds).ToString());
                            System.Threading.Thread.Sleep(60100 - (int)watch.ElapsedMilliseconds);
                            watch.Restart();
                        }

                        //Setar o Timeout do client quando é API BASICA
                        //client.Timeout = TimeSpan.FromMilliseconds(5000);
                        HttpResponseMessage response = await client.GetAsync(string.Empty);

                        //var rateLimit = response.Headers.FirstOrDefault(x => x.Key == "X-RateLimit-Limit");
                        //var retryAfter = response.Headers.FirstOrDefault(x => x.Key == "RetryAfter"); // A API do ReceitaWS infelizmente não retorna um valor de retryAfter, então temos que usar um sleep num valor padrão.
                        var rateLimit_Remaining = response.Headers.FirstOrDefault(x => x.Key == "X-RateLimit-Remaining");


                        if (rateLimit_Remaining.Value != null)
                        {
                            int temp;
                            int.TryParse(rateLimit_Remaining.Value.First(), out temp);
                            RateLimit_Remaining = temp;
                        }
                        else
                        {
                            RateLimit_Remaining = 3;
                        }


                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            receita = (FornecedorInfo)JsonConvert.DeserializeObject(responseString, typeof(FornecedorInfo));
                        }
                        else
                        {
                            if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                            {
                                Console.WriteLine(response.ReasonPhrase);
                                System.Threading.Thread.Sleep(1000);
                            }
                            else
                            {
                                InserirControle(1, item["cnpj_cpf"].ToString(), response.ReasonPhrase.ToString());
                            }
                            
                            continue;
                        }
                    }

                }
                catch (Exception ex)
                {
                    //if (receita == null)
                    //{
                    //    receita = new ReceitaWSData();
                    //    receita.status = ReceitaWSData.STATUS_ERROR;
                    //    receita.ultima_atualizacao = DateTime.Now.ToString(DateMask_UltimaAtualizacao);

                    //    if (ex is AggregateException)
                    //    {
                    //        receita.message = "API Básica -- Gateway Time-out -- Exception: " + ex.InnerException.Message;
                    //        receita.ControleProcessamento = ReceitaWSData.ControleProcessamentoReceitaWS.Ignorar;
                    //    }
                    //    else
                    //    {
                    //        receita.message = ex.Message;
                    //    }
                    //}
                    InserirControle(1, item["cnpj_cpf"].ToString(), ex.GetBaseException().Message);
                    continue;
                }

                string strSql2;
                using (var banco = new AppDb())
                {
                    //if (receita?.status == null)
                    //{
                    //    Thread.Sleep(60000); //1 MINUTO
                    //    continue;
                    //}

                    if (receita.status == "OK")
                    {

                        banco.BeginTransaction();

                        try
                        {
                            if (item["id_fornecedor"] != DBNull.Value)
                            {
                                banco.AddParameter("@id_fornecedor", item["id_fornecedor"]);

                                banco.ExecuteNonQuery(@"
                                delete from fornecedor_info where id_fornecedor=@id_fornecedor;
                                delete from fornecedor_atividade_secundaria where id_fornecedor=@id_fornecedor;
                                delete from fornecedor_socio where id_fornecedor=@id_fornecedor;
                            ");
                            }

                            var strSql =
                                @"insert into fornecedor_info(
							    id_fornecedor,
							    cnpj,
								tipo,
							    nome,
							    data_de_abertura,
							    nome_fantasia,
							    id_fornecedor_atividade_principal,
							    id_fornecedor_natureza_juridica,
							    logradouro,
							    numero,
							    complemento,
							    cep,
							    bairro,
							    municipio,
							    estado,
							    endereco_eletronico,
							    telefone,
							    ente_federativo_responsavel,
							    situacao_cadastral,
							    data_da_situacao_cadastral,
							    motivo_situacao_cadastral,
							    situacao_especial,
							    data_situacao_especial,
							    capital_social,
								obtido_em,
                                ip_colaborador
						    ) values (
							    @id_fornecedor,
							    @cnpj,
								@tipo,
							    @nome,
							    @data_de_abertura,
							    @nome_fantasia,
							    @id_fornecedor_atividade_principal,
							    @id_fornecedor_natureza_juridica,
							    @logradouro,
							    @numero,
							    @complemento,
							    @cep,
							    @bairro,
							    @municipio,
							    @estado,
							    @endereco_eletronico,
							    @telefone,
							    @ente_federativo_responsavel,
							    @situacao_cadastral,
							    @data_da_situacao_cadastral,
							    @motivo_situacao_cadastral,
							    @situacao_especial,
							    @data_situacao_especial,
							    @capital_social,
								@obtido_em,
                                @ip_colaborador
						    )";

                            banco.AddParameter("@id_fornecedor", item["id"]);
                            banco.AddParameter("@cnpj", item["cnpj_cpf"]);
                            banco.AddParameter("@tipo", receita.tipo);
                            banco.AddParameter("@nome", receita.nome);
                            banco.AddParameter("@data_de_abertura", ParseDate(receita.abertura));
                            banco.AddParameter("@nome_fantasia", receita.fantasia);

                            if (receita.atividade_principal != null && receita.atividade_principal.Count > 0)
                            {
                                var drAt = LocalizaInsereAtividade(dtFornecedoresAtividade, receita.atividade_principal[0]);
                                banco.AddParameter("@id_fornecedor_atividade_principal", drAt["id"]);
                            }
                            else
                            {
                                banco.AddParameter("@id_fornecedor_atividade_principal", DBNull.Value);
                            }

                            if (receita.atividade_principal != null)
                            {
                                var drNj =
                                    dtFornecedoresNatJu.Select("codigo='" + receita.natureza_juridica.Split(' ')[0] + "'");
                                banco.AddParameter("@id_fornecedor_natureza_juridica",
                                    drNj.Length > 0 ? drNj[0]["id"] : DBNull.Value);
                            }
                            else
                            {
                                banco.AddParameter("@id_fornecedor_natureza_juridica", DBNull.Value);
                            }

                            banco.AddParameter("@logradouro", receita.logradouro);
                            banco.AddParameter("@numero", receita.numero);
                            banco.AddParameter("@complemento", receita.complemento);
                            banco.AddParameter("@cep", receita.cep);
                            banco.AddParameter("@bairro", receita.bairro);
                            banco.AddParameter("@municipio", receita.municipio);
                            banco.AddParameter("@estado", receita.uf);
                            banco.AddParameter("@endereco_eletronico", receita.email);
                            banco.AddParameter("@telefone", receita.telefone);
                            banco.AddParameter("@ente_federativo_responsavel", receita.efr);
                            banco.AddParameter("@situacao_cadastral", receita.situacao);
                            banco.AddParameter("@data_da_situacao_cadastral", ParseDate(receita.data_situacao));
                            banco.AddParameter("@motivo_situacao_cadastral", receita.motivo_situacao);
                            banco.AddParameter("@situacao_especial", receita.situacao_especial);
                            banco.AddParameter("@data_situacao_especial", ParseDate(receita.data_situacao_especial));
                            banco.AddParameter("@capital_social", ObterValor(receita.capital_social));
                            banco.AddParameter("@obtido_em", ParseDate(receita.ultima_atualizacao));
                            banco.AddParameter("@ip_colaborador", DateTime.Now.ToString("yyMMdd"));

                            banco.ExecuteNonQuery(strSql);


                            banco.AddParameter("@id", item["id"]);
                            banco.AddParameter("@nome", receita.nome);
                            banco.ExecuteNonQuery(@"update fornecedor set nome=@nome where id=@id");


                            strSql2 = @"insert into fornecedor_atividade_secundaria values (@id_fornecedor_info, @id_fornecedor_atividade)";
                            foreach (var atividadesSecundaria in receita.atividades_secundarias)
                            {
                                banco.ClearParameters();

                                try
                                {
                                    var drAt = LocalizaInsereAtividade(dtFornecedoresAtividade, atividadesSecundaria);
                                    banco.AddParameter("@id_fornecedor_info", item["id"]);
                                    banco.AddParameter("@id_fornecedor_atividade", drAt["id"]);

                                    banco.ExecuteNonQuery(strSql2);
                                }
                                catch (MySqlException ex)
                                {
                                    if (!ex.Message.Contains("Duplicate entry")) throw;
                                }
                            }

                            strSql2 =
                                @"insert into fornecedor_socio 
                                (id_fornecedor, nome, pais_origem, id_fornecedor_socio_qualificacao, nome_representante, id_fornecedor_socio_representante_qualificacao) 
                            values 
                                (@id_fornecedor, @nome, @pais_origem, @id_fornecedor_socio_qualificacao, @nome_representante, @id_fornecedor_socio_representante_qualificacao)";

                            foreach (var qsa in receita.qsa)
                            {
                                banco.ClearParameters();

                                banco.AddParameter("@id_fornecedor", item["id"]);
                                banco.AddParameter("@nome", qsa.nome);
                                banco.AddParameter("@pais_origem", qsa.pais_origem);
                                banco.AddParameter("@id_fornecedor_socio_qualificacao", qsa.qual.Split('-')[0]);

                                banco.AddParameter("@nome_representante", qsa.nome_rep_legal);
                                banco.AddParameter("@id_fornecedor_socio_representante_qualificacao",
                                    !string.IsNullOrEmpty(qsa.qual_rep_legal) && qsa.qual_rep_legal.Contains("-") ? (object)qsa.qual_rep_legal.Split('-')[0] : DBNull.Value);

                                banco.ExecuteNonQuery(strSql2);
                            }

                            totalImportados++;

                            if (receita.situacao != "ATIVA")
                            {
                                var enviarAlerta = false;

                                if (!string.IsNullOrEmpty(receita.data_situacao_especial))
                                {
                                    banco.AddParameter("@id", Convert.ToInt32(item["id"]));
                                    var ulimaNota = banco.ExecuteScalar(@"
SELECT MAX(DATA) as data FROM (
	SELECT MAX(d.data_emissao) AS data FROM cf_despesa d WHERE d.id_fornecedor = @id
	UNION
	SELECT MAX(d.data_documento) FROM sf_despesa d WHERE d.id_fornecedor = @id
	UNION
	SELECT MAX(d.data) FROM cl_despesa d WHERE d.id_fornecedor = @id
)
");
                                    if (ulimaNota != null && Convert.ToDateTime(ulimaNota) > Convert.ToDateTime(receita.data_situacao_especial))
                                        enviarAlerta = true;
                                }

                                if (enviarAlerta)
                                {
                                    strInfoAdicional.Append("<p>Empresa inativa importada:" + item["id"].ToString() + " - " + receita.cnpj + " - " + receita.nome + "</p>");

                                    telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.net.br/fornecedor/{item["id"]}'>{receita.cnpj} - {receita.nome}</a>";
                                    telegram.SendMessage(telegraMessage);
                                }
                            }

                            banco.CommitTransaction();

                            InserirControle(0, item["cnpj_cpf"].ToString(), "");
                            Console.WriteLine($"Atualizando CNPJ: {receita.cnpj} {receita.nome} - " + i);
                        }
                        catch (Exception)
                        {
                            banco.RollBackTransaction();
                        }
                    }
                    else
                    {
                        InserirControle(2, item["cnpj_cpf"].ToString(), receita.message);

                        strInfoAdicional.Append("<p>Empresa invalida importada:" + item["id"].ToString() + " - " + receita.cnpj + " - " + item["nome"].ToString() + "; Motivo: " + receita.message + "</p>");

                        telegraMessage.Text = $"Empresa Inativa: <a href='https://ops.net.br/fornecedor/{item["id"]}'>{receita.cnpj} - {receita.nome}</a>; Motivo: {receita.message}";
                        telegram.SendMessage(telegraMessage);
                    }
                }
            }

            using (var banco = new AppDb())
            {
                banco.ExecuteNonQuery(@"
					update fornecedor_info set nome_fantasia=null where nome_fantasia = '' or nome_fantasia = '********';
					update fornecedor_info set logradouro=null where logradouro = '' or logradouro = '********';
					update fornecedor_info set numero=null where numero = '' or numero = '********';
					update fornecedor_info set complemento=null where complemento = '' or complemento = '********';
					update fornecedor_info set cep=null where cep = '' or cep = '********';
					update fornecedor_info set bairro=null where bairro = '' or bairro = '********';
					update fornecedor_info set municipio=null where municipio = '' or municipio = '********';
					update fornecedor_info set estado=null where estado = '' or estado = '**';
					update fornecedor_info set endereco_eletronico=null where endereco_eletronico = '' or endereco_eletronico = '********';
					update fornecedor_info set telefone=null where telefone = '' or telefone = '********';
					update fornecedor_info set ente_federativo_responsavel=null where ente_federativo_responsavel = '' or ente_federativo_responsavel = '********';
					update fornecedor_info set motivo_situacao_cadastral=null where motivo_situacao_cadastral = '' or motivo_situacao_cadastral = '********';
					update fornecedor_info set situacao_especial=null where situacao_especial = '' or situacao_especial = '********';
				");
            }

            return string.Format("<p>{0} de {1} fornecedores novos importados</p>", totalImportados, dtFornecedores.Rows.Count) + strInfoAdicional.ToString();
        }

        private static void InserirControle(int controle, string cnpj_cpf, string mensagem)
        {
            using (var banco = new AppDb())
            {
                banco.AddParameter("@cnpj_cpf", cnpj_cpf);
                banco.AddParameter("@controle", controle);
                banco.AddParameter("@mensagem", mensagem);

                banco.ExecuteNonQuery(@"update fornecedor set controle=@controle, mensagem=@mensagem where cnpj_cpf=@cnpj_cpf;");
            }
        }

        private static DataRow LocalizaInsereAtividade(DataTable dtFornecedoresAtividade, IAtividade atividadesSecundaria)
        {
            var drs = dtFornecedoresAtividade.Select("codigo='" + atividadesSecundaria.code + "'");

            var dr = dtFornecedoresAtividade.NewRow();

            if (drs.Length == 0)
            {
                using (var banco = new AppDb())
                {
                    var strSql =
                        @"insert into fornecedor_atividade (codigo, descricao) values (@codigo, @descricao); SELECT LAST_INSERT_ID();";
                    banco.AddParameter("@codigo", atividadesSecundaria.code);
                    banco.AddParameter("@descricao", atividadesSecundaria.text);

                    dr["id"] = Convert.ToInt32(banco.ExecuteScalar(strSql));
                    dr["codigo"] = atividadesSecundaria.code;
                    dr["descricao"] = atividadesSecundaria.text;

                    dtFornecedoresAtividade.Rows.Add(dr);

                    return dr;
                }
            }

            return drs[0];
        }

        private static object ObterValor(object d)
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

        private static object ParseDate(object d)
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
    }

    #region ReceitaWS DTO
    public interface IAtividade
    {
        string text { get; set; }
        string code { get; set; }
    }

    public class AtividadePrincipal : IAtividade
    {
        public string text { get; set; }
        public string code { get; set; }
    }

    public class AtividadesSecundaria : IAtividade
    {
        public string text { get; set; }
        public string code { get; set; }
    }

    public class Qsa
    {
        public string qual { get; set; }
        public string nome { get; set; }
        public string pais_origem { get; set; }
        public string nome_rep_legal { get; set; }
        public string qual_rep_legal { get; set; }
    }

    public class Extra
    {
    }

    public class FornecedorInfo
    {
        public List<AtividadePrincipal> atividade_principal { get; set; }
        public string data_situacao { get; set; }
        public string complemento { get; set; }
        public string nome { get; set; }
        public string uf { get; set; }
        public string telefone { get; set; }
        public List<AtividadesSecundaria> atividades_secundarias { get; set; }
        public List<Qsa> qsa { get; set; }
        public string situacao { get; set; }
        public string bairro { get; set; }
        public string logradouro { get; set; }
        public string numero { get; set; }
        public string cep { get; set; }
        public string municipio { get; set; }
        public string abertura { get; set; }
        public string natureza_juridica { get; set; }
        public string fantasia { get; set; }
        public string cnpj { get; set; }
        public string ultima_atualizacao { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string tipo { get; set; }
        public string email { get; set; }
        public string efr { get; set; }
        public string motivo_situacao { get; set; }
        public string situacao_especial { get; set; }
        public string data_situacao_especial { get; set; }
        public string capital_social { get; set; }
        public Extra extra { get; set; }
    }
    #endregion ReceitaWS DTO
}