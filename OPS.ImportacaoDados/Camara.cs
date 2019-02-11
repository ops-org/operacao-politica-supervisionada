/*
 * Created by SharpDevelop.
 * User: Tex Killer
 * Date: 18/01/2016
 * Time: 06:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using OPS.Core;
using RestSharp;

namespace OPS.ImportacaoDados
{
    public static class Camara
    {
        //private static readonly string[] ColunasCEAP = {
        //    "txNomeParlamentar", "numeroDeputadoID", "numeroCarteiraParlamentar", "legislatura", "siglaUF",
        //    "siglaPartido",        "codigoLegislatura", "numeroSubCota",     "txtDescricao", "numeroEspecificacaoSubCota",
        //    "txtDescricaoEspecificacao", "fornecedor", "cnpjCPF",     "numero", "tipoDocumento",
        //    "dataEmissao", "valorDocumento", "valorGlosa", "valorLiquido", "mes",
        //    "ano", "parcela", "passageiro", "trecho", "lote",
        //    "ressarcimento", "restituicao",       "numeroDeputadoID", "idDocumento"
        //};

        //public static void ValidarLinkRecibos()
        //{
        //    using (var banco = new Banco())
        //    {
        //        DataTable dtDocumentos = banco.GetTable(
        //            "select id, id_cf_deputado, ano, id_documento from cf_despesa where id > 1506033 and id_documento is not null and link_documento is null");

        //        foreach (DataRow dr in dtDocumentos.Rows)
        //        {
        //            string downloadUrl =
        //                string.Format("http://www.camara.gov.br/cota-parlamentar/documentos/publ/{0}/{1}/{2}.pdf",
        //                dr["id_cf_deputado"], dr["ano"], dr["id_documento"]);

        //            try
        //            {
        //                var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

        //                request.UserAgent = "Other";
        //                request.Method = "HEAD";
        //                request.ContentType = "application/json;charset=UTF-8";
        //                request.Timeout = 1000000;

        //                using (var resp = request.GetResponse())
        //                {
        //                    long contentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
        //                    if (contentLength == 0) continue;
        //                }
        //            }
        //            catch (System.Net.WebException ex)
        //            {
        //                if (!ex.Message.Contains("404"))
        //                    Console.WriteLine(ex.Message);

        //                continue;
        //            }

        //            banco.AddParameter("link_documento", downloadUrl);
        //            banco.AddParameter("id", dr["id"]);
        //            banco.ExecuteNonQuery("UPDATE cf_despesa SET link_documento=? WHERE id=?");
        //        }
        //    }
        //}

        /// <summary>
        /// Importar mandatos
        /// </summary>
        public static void ImportarMandatos()
        {
            // http://www2.camara.leg.br/transparencia/dados-abertos/dados-abertos-legislativo/webservices/deputados
            // http://www.camara.leg.br/internet/deputado/DeputadosXML_52a55.zip

            var doc = new XmlDocument();
            doc.Load(@"D:\GitHub\operacao-politica-supervisionada\OPS\temp\DeputadosXML_52a55\Deputados.xml");
            XmlNode deputados = doc.DocumentElement;

            var deputado = deputados.SelectNodes("Deputados/Deputado");
            var sqlFields = new StringBuilder();
            var sqlValues = new StringBuilder();

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery("TRUNCATE TABLE cf_mandato_temp");

                foreach (XmlNode fileNode in deputado)
                {
                    sqlFields.Clear();
                    sqlValues.Clear();

                    foreach (XmlNode item in fileNode.SelectNodes("*"))
                    {
                        sqlFields.Append($",{item.Name}");

                        sqlValues.Append($",@{item.Name}");
                        banco.AddParameter(item.Name, item.InnerText);
                    }

                    banco.ExecuteNonQuery("INSERT cf_mandato_temp (" + sqlFields.ToString().Substring(1) + ")  values (" +
                                          sqlValues.ToString().Substring(1) + ")");
                }

                banco.ExecuteNonQuery(@"
                    SET SQL_BIG_SELECTS=1;

					INSERT INTO cf_mandato (
	                    id_cf_deputado,
	                    id_legislatura,
	                    id_carteira_parlamantar,
	                    id_estado, 
	                    id_partido,
                        condicao
                    )
                    select 
	                    mt.numeroDeputadoID,
	                    mt.numLegislatura,
	                    mt.Matricula,
	                    e.id, 
	                    p.id,
	                    mt.Condicao
                    FROM cf_mandato_temp mt
                    left join partido p on p.sigla = mt.LegendaPartidoEleito
                    left join estado e on e.sigla = mt.UFEleito
                    /* Inserir apenas faltantes */
                    LEFT JOIN cf_mandato m ON m.id_cf_deputado = mt.numeroDeputadoID
	                    AND m.id_legislatura = mt.numLegislatura 
	                    AND m.id_carteira_parlamantar = mt.Matricula
                    WHERE m.id is null;
                    
                    SET SQL_BIG_SELECTS=0;
				");

                //banco.ExecuteNonQuery("TRUNCATE TABLE cf_mandato_temp;");
            }
        }

        /// <summary>
        /// Atualiza informações dos deputados em exercício na Câmara dos Deputados (1)
        /// </summary>
        public static string AtualizaInfoDeputados()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string retorno = string.Empty;
            // http://www2.camara.leg.br/transparencia/dados-abertos/dados-abertos-legislativo/webservices/deputados

            var doc = new XmlDocument();
            var client = new RestClient("http://www.camara.leg.br/SitCamaraWS/Deputados.asmx");
            var request = new RestRequest("ObterDeputados", Method.GET);
            var response = client.Execute(request);
            doc.LoadXml(response.Content);
            XmlNode deputados = doc.DocumentElement;

            var deputado = deputados.SelectNodes("*");
            var sqlFields = new StringBuilder();
            var sqlValues = new StringBuilder();

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp");

                foreach (XmlNode fileNode in deputado)
                {
                    sqlFields.Clear();
                    sqlValues.Clear();

                    foreach (XmlNode item in fileNode.SelectNodes("*"))
                    {
                        if (item.Name != "comissoes")
                        {
                            sqlFields.Append(string.Format(",{0}", item.Name));

                            sqlValues.Append(string.Format(",@{0}", item.Name));
                            if (item.Name == "nome" || item.Name == "nomeParlamentar")
                            {
                                banco.AddParameter(item.Name, textInfo.ToTitleCase(item.InnerText));
                            }
                            else
                            {
                                banco.AddParameter(item.Name, item.InnerText);
                            }

                        }
                    }

                    banco.ExecuteNonQuery("INSERT cf_deputado_temp (" + sqlFields.ToString().Substring(1) +
                                          ")  values (" + sqlValues.ToString().Substring(1) + ")");
                }


                banco.ExecuteNonQuery(@"
                    SET SQL_BIG_SELECTS=1;

					update cf_deputado d
					left join cf_deputado_temp dt on dt.numeroDeputadoID = d.id_deputado
					left join partido p on p.sigla = dt.partido
					left join estado e on e.sigla = dt.uf
					set
                        -- d.condicao = dt.condicao,
						-- d.nome_civil = dt.nome,
						-- d.nome_parlamentar = dt.nomeParlamentar,
						-- d.url_foto = dt.urlFoto,
						-- d.sexo = LEFT(dt.sexo, 1),
						-- d.id_estado = e.id,
						-- d.id_partido = p.id,
						-- d.gabinete = dt.gabinete,
						-- d.anexo = dt.anexo,
						-- d.fone = dt.fone,
						 d.email = dt.email
					where dt.nomeParlamentar is not null;

                    SET SQL_BIG_SELECTS=0;
				");

                if (banco.Rows > 0)
                {
                    retorno = "<p>" + banco.Rows + "+ Mandato<p>";
                }

                //banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp");

                return retorno;
            }
        }

        /// <summary>
        /// Atualiza informações dos deputados em exercício na Câmara dos Deputados (2)
        /// </summary>
        public static string AtualizaInfoDeputadosCompleto(bool cargaInicial = false)
        {
            string retorno = string.Empty;
            // http://www2.camara.leg.br/transparencia/dados-abertos/dados-abertos-legislativo/webservices/deputados

            var sqlFields = new StringBuilder();
            var sqlValues = new StringBuilder();

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp_detalhes");

                var dt = banco.GetTable("SELECT id from cf_deputado" + (!cargaInicial ? " WHERE situacao <> 'Fim de Mandato'" : ""));

                foreach (DataRow dr in dt.Rows)
                {
                    // Retorna detalhes dos deputados com histórico de participação em comissões, períodos de exercício, filiações partidárias e lideranças.
                    var doc = new XmlDocument();
                    var client = new RestClient("http://www.camara.leg.br/SitCamaraWS/Deputados.asmx/");
                    var request = new RestRequest("ObterDetalhesDeputado?numLegislatura=" + (!cargaInicial ? "56" : "") + "&numeroDeputadoID=" + dr["id"].ToString(), Method.GET);

                    try
                    {
                        var response = client.Execute(request);
                        doc.LoadXml(response.Content);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(5000);

                        try
                        {
                            var response = client.Execute(request);
                            doc.LoadXml(response.Content);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    XmlNode deputados = doc.DocumentElement;
                    var deputado = deputados.SelectNodes("*");

                    foreach (XmlNode fileNode in deputado)
                    {
                        sqlFields.Clear();
                        sqlValues.Clear();

                        foreach (XmlNode item in fileNode.ChildNodes)
                        {
                            if (item.Name == "comissoes") break;
                            if ((item.Name != "partidoAtual") && (item.Name != "gabinete"))
                            {
                                object value;
                                if (item.Name.StartsWith("data"))
                                    if (!string.IsNullOrEmpty(item.InnerText))
                                        value = DateTime.Parse(item.InnerText).ToString("yyyy-MM-dd");
                                    else
                                        value = DBNull.Value;
                                else
                                    value = item.InnerText;

                                sqlFields.Append($",{item.Name}");
                                sqlValues.Append($",@{item.Name}");
                                banco.AddParameter(item.Name, value);
                            }
                            else
                            {
                                foreach (XmlNode item2 in item.ChildNodes)
                                {
                                    if ((item2.Name == "idPartido") || (item2.Name == "nome")) continue;


                                    sqlFields.Append($",{item2.Name}");
                                    sqlValues.Append($",@{item2.Name}");
                                    banco.AddParameter(item2.Name, item2.InnerText);
                                }
                            }
                        }

                        try
                        {
                            banco.ExecuteNonQuery("INSERT cf_deputado_temp_detalhes (" +
                                                   sqlFields.ToString().Substring(1) + ")  values (" +
                                                   sqlValues.ToString().Substring(1) + ")");

                            break;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                banco.ExecuteNonQuery(@"
                	update cf_deputado d
                	left join (
                		select		
                			numeroDeputadoID,
                			idParlamentarDeprecated,
                			nomeCivil,
                			nomeParlamentarAtual,
                			sexo,
                			nomeProfissao,
                			dataNascimento,
                			ufRepresentacaoAtual,
                			max(email) as email,
                			max(sigla) as sigla,
                			max(numero) as numero,
                			max(anexo) as anexo,
                			max(telefone) as telefone,
                			max(dataFalecimento) as dataFalecimento
                		from cf_deputado_temp_detalhes
                		group by 1, 2, 3, 4, 5, 6, 7, 8
                	) dt on dt.numeroDeputadoID = d.id_cf_deputado
                	left join partido p on p.sigla = dt.sigla
                	left join estado e on e.sigla = dt.ufRepresentacaoAtual
                	set
                		d.nome_civil = dt.nomeCivil,
                		d.nome_parlamentar = dt.nomeParlamentarAtual,
                		d.sexo = LEFT(dt.sexo, 1),
                		d.id_estado = e.id,
                		d.id_partido = p.id,
                		d.gabinete = dt.numero,
                		d.anexo = dt.anexo,
                		d.fone = dt.telefone,
                		d.email = dt.email,
                		d.profissao = dt.nomeProfissao,
                		d.nascimento = dt.dataNascimento,
                		d.falecimento = dt.dataFalecimento
                	where dt.nomeParlamentarAtual is not null;
                ");

                if (banco.Rows > 0)
                {
                    retorno = "<p>" + banco.Rows + "+ Mandato<p>";
                }

                //banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp_detalhes;");
            }

            return retorno;
        }

        public static string ImportaPresencasDeputados()
        {
            var sb = new StringBuilder();
            Console.WriteLine("Iniciando Importação de presenças");


            string checksum = string.Empty;

            DataTable dtMandatos;
            DataTable dtSessoes;
            using (var banco = new Banco())
            {
                dtMandatos = banco.GetTable("select id_cf_deputado, id_legislatura, id_carteira_parlamantar from cf_mandato");
                dtSessoes = banco.GetTable("select id, id_legislatura, data, inicio, tipo, numero, checksum from cf_sessao");
            }

            //Carregar a partir da legislatura 53 (2007/02/01). Existem dados desde 24/02/99
            DateTime dtPesquisa = Padrao.DeputadoFederalPresencaUltimaAtualizacao.Date; //new DateTime(2007, 2, 1);
            if (dtPesquisa == DateTime.MinValue) dtPesquisa = new DateTime(2016, 2, 9);
            var dtNow = DateTime.Now.Date;
            //var dtUltimaIntegracao = dtPesquisa.AddDays(-7);
            int presencas_importadas = 0;

            dtPesquisa = dtPesquisa.AddDays(-30);

            while (true)
            {
                dtPesquisa = dtPesquisa.AddDays(1);
                if (dtPesquisa > dtNow) break;

                var doc = new XmlDocument();
                IRestResponse response;
                var client = new RestClient("http://www.camara.leg.br/SitCamaraWS/sessoesreunioes.asmx/");
                var request =
                    new RestRequest(
                        "ListarPresencasDia?data={data}&numLegislatura=&numMatriculaParlamentar=&siglaPartido=&siglaUF=",
                        Method.GET);

                request.AddUrlSegment("data", dtPesquisa.ToString("dd/MM/yyyy"));


                //if (dtPesquisa < dtUltimaIntegracao && dtSessoes.Select().All(r => Convert.ToDateTime(r["data"]) != dtPesquisa))
                //{
                //    Console.WriteLine("Não Houve sessão no dia: " + dtPesquisa.ToShortDateString());
                //    sb.AppendFormat("<p>Não Houve sessão no dia: {0:dd/MM/yyyy}</p>", dtPesquisa);

                //    // Não houve sessão no dia.
                //    continue;
                //}

                try
                {
                    response = client.Execute(request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro:" + ex.Message);

                    if (ex.Message.Contains("timeout"))
                    {
                        Thread.Sleep(5000);

                        dtPesquisa = dtPesquisa.AddDays(-1);
                        continue;
                    }
                    else
                    {
                        sb.AppendFormat("<p>Erro:{0}</p>", ex.Message);

                        return sb.ToString();
                    }
                }

                if (!response.Content.Contains("qtdeSessoesDia"))
                {
                    //sb.AppendFormat("<p>Não Houve sessão no dia: {0:dd/MM/yyyy}</p>", dtPesquisa);
                    continue;
                }

                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    checksum = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(response.Content)));
                    List<DataRow> drSessoes = dtSessoes.Select().Where(r => Convert.ToDateTime(r["data"]) == dtPesquisa).ToList();

                    if (drSessoes.Count > 0)
                    {
                        if (drSessoes[0]["checksum"].ToString() == checksum)
                        {
                            sb.AppendFormat("<p>Sem alterações na sessão no dia: {0:dd/MM/yyyy}</p>", dtPesquisa);
                            continue;
                        }
                        else
                        {
                            using (var banco = new Banco())
                            {
                                foreach (DataRow dr in drSessoes)
                                {
                                    var id_cf_sessao = Convert.ToInt32(dr["id"]);

                                    banco.AddParameter("id", id_cf_sessao);
                                    banco.ExecuteNonQuery("delete from cf_sessao where id=@id");

                                    banco.AddParameter("id", id_cf_sessao);
                                    banco.ExecuteNonQuery("delete from cf_sessao_presenca where id_cf_sessao=@id");
                                }
                            }
                        }
                    }
                }

                doc.LoadXml(response.Content);

                Console.WriteLine("Importando presenças do dia: " + dtPesquisa.ToShortDateString());
                sb.AppendFormat("<p>Importando presenças do dia: {0:dd/MM/yyyy}</p>", dtPesquisa);
                XmlNode dia = doc.DocumentElement;

                var lstSessaoDia = new Dictionary<string, int>();

                using (var banco = new Banco())
                {
                    var count = Convert.ToInt32(dia["qtdeSessoesDia"].InnerText);
                    for (var i = 0; i < count; i++)
                    {
                        presencas_importadas++;
                        var temp = dia.SelectNodes("parlamentares/parlamentar/sessoesDia/sessaoDia").Item(i);

                        var inicio = temp["inicio"].InnerText;

                        if (lstSessaoDia.ContainsKey(inicio)) continue; // ignorar registro duplicado

                        var descricao =
                            temp["descricao"].InnerText
                                .Replace("N º", "").Replace("Nº", "")
                                .Replace("SESSÃO PREPARATÓRIA", "SESSÃO_PREPARATÓRIA")
                                .Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        banco.AddParameter("id_legislatura", dia["legislatura"].InnerText);
                        banco.AddParameter("data", DateTime.Parse(dia["data"].InnerText));
                        banco.AddParameter("inicio", DateTime.Parse(inicio));

                        //1=> ORDINÁRIA, 2=> EXTRAORDINÁRIA, 3=> SESSÃO PREPARATÓRIA
                        var tipo = 0;
                        switch (descricao[0])
                        {
                            case "ORDINÁRIA":
                            case "ORDINARIA":
                                tipo = 1;
                                break;
                            case "EXTRAORDINÁRIA":
                            case "EXTRAORDINARIA":
                                tipo = 2;
                                break;
                            case "SESSÃO_PREPARATÓRIA":
                            case "SESSÃO_PREPARATORIA":
                                tipo = 3;
                                break;
                            default:
                                throw new NotImplementedException("");
                        }

                        banco.AddParameter("tipo", tipo);
                        banco.AddParameter("numero", descricao[1]);
                        banco.AddParameter("checksum", checksum);

                        var id_cf_secao = Convert.ToInt32(banco.ExecuteScalar(
                            @"INSERT cf_sessao (
								id_legislatura, data, inicio, tipo, numero, checksum
							) values ( 
								@id_legislatura, @data, @inicio, @tipo, @numero, @checksum
							); 
							
							SELECT LAST_INSERT_ID();"));

                        lstSessaoDia.Add(inicio, id_cf_secao);
                    }

                    var parlamentares = dia.SelectNodes("parlamentares/parlamentar");

                    foreach (XmlNode parlamentar in parlamentares)
                    {
                        var sessoesDia = parlamentar.SelectNodes("sessoesDia/sessaoDia");

                        foreach (XmlNode sessaoDia in sessoesDia)
                        {
                            string id_cf_deputado;
                            var drMqandato = dtMandatos.Select(
                                string.Format("id_legislatura={0} and id_carteira_parlamantar={1}",
                                    dia["legislatura"].InnerText,
                                    parlamentar["carteiraParlamentar"].InnerText
                                )
                            );

                            if (drMqandato.Length > 0)
                            {
                                id_cf_deputado = drMqandato[0]["id_cf_deputado"].ToString();
                            }
                            else
                            {
                                var nome_parlamentar = parlamentar["nomeParlamentar"].InnerText.Split('-');
                                var sigla_partido = "";
                                var sigla_estado = "";
                                try
                                {
                                    var temp = nome_parlamentar[1].Split('/');
                                    sigla_partido = temp[0];
                                    sigla_estado = temp[1];
                                }
                                catch (Exception e)
                                {
                                    var x = e;
                                }

                                try
                                {
                                    banco.AddParameter("nome_parlamentar", nome_parlamentar[0]);
                                    id_cf_deputado = banco.ExecuteScalar("SELECT id FROM cf_deputado where nome_parlamentar like @nome_parlamentar").ToString();
                                }
                                catch (Exception e)
                                {
                                    sb.AppendFormat("<p>1. Parlamentar '{0}' não consta na base de dados, e foi ignorado na importação de presenças. Legislatura: {1}, Carteira: {2}.</p>",
                                        parlamentar["nomeParlamentar"].InnerText, dia["legislatura"].InnerText, parlamentar["carteiraParlamentar"].InnerText);
                                    continue;
                                }

                                banco.AddParameter("id_cf_deputado", id_cf_deputado);
                                banco.AddParameter("id_legislatura", dia["legislatura"].InnerText);
                                banco.AddParameter("id_carteira_parlamantar", parlamentar["carteiraParlamentar"].InnerText);
                                banco.AddParameter("sigla_estado", sigla_estado);
                                banco.AddParameter("sigla_partido", sigla_partido);

                                try
                                {
                                    banco.ExecuteScalar(
                                        @"INSERT INTO cf_mandato (
	                                    id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido
                                    ) VALUES (
	                                    @id_cf_deputado
                                        , @id_legislatura
                                        , @id_carteira_parlamantar
                                        , (SELECT id FROM estado where sigla like @sigla_estado)
                                        , (SELECT id FROM partido where sigla like @sigla_partido)
                                    );");

                                    // generate the data you want to insert
                                    var toInsert = dtMandatos.NewRow();
                                    toInsert["id_cf_deputado"] = id_cf_deputado;
                                    toInsert["id_legislatura"] = dia["legislatura"].InnerText;
                                    toInsert["id_carteira_parlamantar"] = parlamentar["carteiraParlamentar"].InnerText;

                                    // insert in the desired place
                                    dtMandatos.Rows.Add(toInsert);
                                }
                                catch (Exception)
                                {
                                    // parlamentar não existe na base
                                    Console.WriteLine(parlamentar["nomeParlamentar"].InnerText + "/" + dia["legislatura"].InnerText + "/" + parlamentar["carteiraParlamentar"].InnerText);

                                    sb.AppendFormat("<p>2. Parlamentar '{0}' não consta na base de dados, e foi ignorado na importação de presenças. Legislatura: {1}, Carteira: {2}.</p>",
                                        parlamentar["nomeParlamentar"].InnerText, dia["legislatura"].InnerText, parlamentar["carteiraParlamentar"].InnerText);
                                }
                            }


                            banco.AddParameter("id_cf_sessao", lstSessaoDia[sessaoDia["inicio"].InnerText]);
                            banco.AddParameter("id_cf_deputado", Convert.ToInt32(id_cf_deputado));

                            banco.AddParameter("presente", sessaoDia["frequencia"].InnerText == "Presença" ? 1 : 0);
                            banco.AddParameter("justificativa", parlamentar["justificativa"].InnerText);
                            banco.AddParameter("presenca_externa", parlamentar["presencaExterna"].InnerText);

                            banco.ExecuteNonQuery(
                                "INSERT cf_sessao_presenca (id_cf_sessao, id_cf_deputado, presente, justificativa, presenca_externa) values (@id_cf_sessao, @id_cf_deputado, @presente, @justificativa, @presenca_externa);");
                        }
                    }
                }
            }

            Padrao.DeputadoFederalPresencaUltimaAtualizacao = DateTime.Now;

            using (var banco = new Banco())
            {
                banco.AddParameter("cf_deputado_presenca_ultima_atualizacao", Padrao.DeputadoFederalPresencaUltimaAtualizacao);
                banco.ExecuteNonQuery(@"UPDATE parametros SET cf_deputado_presenca_ultima_atualizacao=@cf_deputado_presenca_ultima_atualizacao");
            }

            //if (presencas_importadas > 0)
            //{
            //	using (var banco = new Banco())
            //	{
            //		for (int ano = ano_inicio; ano <= dtPesquisa.Year; ano++)
            //		{
            //			banco.AddParameter("ano", ano);
            //			var total_sessoes = Convert.ToInt32(banco.ExecuteScalar(@"SELECT COUNT(1) FROM cf_sessao WHERE year(data)=@ano"));

            //			banco.AddParameter("ano", ano);
            //			banco.AddParameter("total_sessoes", total_sessoes);
            //			banco.ExecuteNonQuery(@"UPDATE cf_legislatura SET total_sessoes=@total_sessoes WHERE ano=@ano");


            //			banco.ExecuteNonQuery(@"UPDATE cf_legislatura SET total_sessoes=@total_sessoes WHERE ano=@ano");
            //		}
            //	}
            //}
            return sb.ToString();
        }

        public static string ImportarDeputados(int legislaturaInicial = 56)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var sb = new StringBuilder();
            int novos = 0;
            int pagina;
            var client = new RestClient("https://dadosabertos.camara.leg.br/api/v2/deputados/");

            //int leg = 55;
            Deputados deputados;

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(@"UPDATE cf_deputado SET id_cf_gabinete=NULL");
            }

            for (int leg = legislaturaInicial; leg <= 56; leg++)
            {
                pagina = 1;
                do
                {
                    var uri = string.Format("?idLegislatura={0}&pagina={1}&itens=100", leg, pagina++);
                    var request = new RestRequest(uri, Method.GET);
                    request.AddHeader("Accept", "application/json");

                    deputados = client.Execute<Deputados>(request).Data;

                    foreach (var deputado in deputados.dados)
                    {
                        using (var banco = new Banco())
                        {
                            banco.AddParameter("id", deputado.id);
                            var count = banco.ExecuteScalar(@"SELECT COUNT(1) FROM cf_deputado WHERE id=@id");

                            request = new RestRequest(deputado.id.ToString(), Method.GET);
                            request.AddHeader("Accept", "application/json");
                            Dados deputadoDetalhes = client.Execute<DeputadoDetalhes>(request).Data.dados;

                            if (deputadoDetalhes == null)
                            {
                                Console.WriteLine(deputado.id);
                                continue;
                            }

                            if (deputadoDetalhes.redeSocial.Any())
                            {
                                Console.WriteLine(string.Join(",", deputadoDetalhes.redeSocial));
                            }

                            int? id_cf_gabinete = null;
                            if (deputadoDetalhes.ultimoStatus.gabinete.sala != null)
                            {
                                banco.AddParameter("sala", deputadoDetalhes.ultimoStatus.gabinete.sala);
                                var id = banco.ExecuteScalar(@"SELECT id from cf_gabinete where sala = @sala");

                                var gabinete = deputadoDetalhes.ultimoStatus.gabinete;

                                if (id == null || Convert.IsDBNull(id))
                                {
                                    banco.AddParameter("id", gabinete.sala);
                                    banco.AddParameter("nome", gabinete.nome);
                                    banco.AddParameter("predio", gabinete.predio);
                                    banco.AddParameter("andar", gabinete.andar);
                                    banco.AddParameter("sala", gabinete.sala);
                                    banco.AddParameter("telefone", gabinete.telefone);

                                    banco.ExecuteNonQuery(@"INSERT INTO cf_gabinete (
                                        id,
                                        nome,
                                        predio,
                                        andar,
                                        sala,
                                        telefone
                                    ) VALUES (
                                        @id,
                                        @nome,
                                        @predio,
                                        @andar,
                                        @sala,
                                        @telefone
                                    )");

                                    id_cf_gabinete = Convert.ToInt32(gabinete.sala);
                                }
                                else
                                {
                                    id_cf_gabinete = Convert.ToInt32(id);

                                    if (leg == 55)
                                    {
                                        banco.AddParameter("nome", gabinete.nome);
                                        banco.AddParameter("predio", gabinete.predio);
                                        banco.AddParameter("andar", gabinete.andar);
                                        banco.AddParameter("telefone", gabinete.telefone);
                                        banco.AddParameter("sala", gabinete.sala);

                                        banco.ExecuteNonQuery(@"UPDATE cf_gabinete SET
                                        nome = @nome,
                                        predio = @predio,
                                        andar = @andar,
                                        telefone = @telefone
                                        WHERE sala = @sala
                                    ");
                                    }
                                }
                            }

                            string situacao;
                            if (leg == 55)
                            {
                                situacao = deputadoDetalhes.ultimoStatus.situacao;
                            }
                            else
                            {
                                situacao = "Fim de Mandato";
                            }

                            if (Convert.ToInt32(count) == 0)
                            {
                                banco.AddParameter("id", deputado.id);
                                banco.AddParameter("sigla_partido", deputadoDetalhes.ultimoStatus.siglaPartido);
                                banco.AddParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf);
                                banco.AddParameter("id_cf_gabinete", id_cf_gabinete);
                                banco.AddParameter("cpf", deputadoDetalhes.cpf);
                                banco.AddParameter("nome_civil", textInfo.ToTitleCase(deputadoDetalhes.nomeCivil));
                                banco.AddParameter("nome_parlamentar", textInfo.ToTitleCase(deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil));
                                banco.AddParameter("sexo", deputadoDetalhes.sexo);
                                banco.AddParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral);
                                banco.AddParameter("situacao", situacao);
                                banco.AddParameter("email", deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower());
                                banco.AddParameter("nascimento", deputadoDetalhes.dataNascimento);
                                banco.AddParameter("falecimento", deputadoDetalhes.dataFalecimento);
                                banco.AddParameter("sigla_estado_nascimento", deputadoDetalhes.ufNascimento);
                                banco.AddParameter("municipio", deputadoDetalhes.municipioNascimento);
                                banco.AddParameter("website", deputadoDetalhes.urlWebsite);
                                banco.AddParameter("escolaridade", deputadoDetalhes.escolaridade);

                                banco.ExecuteNonQuery(@"INSERT INTO cf_deputado (
                                    id,
                                    id_partido, 
                                    id_estado,
                                    id_cf_gabinete,
                                    cpf, 
                                    nome_civil, 
                                    nome_parlamentar, 
                                    sexo, 
                                    condicao, 
                                    situacao, 
                                    email, 
                                    nascimento, 
                                    falecimento, 
                                    id_estado_nascimento,
                                    municipio, 
                                    website,
                                    escolaridade
                                ) VALUES (
                                    @id,
                                    (SELECT id FROM partido where sigla like @sigla_partido), 
                                    (SELECT id FROM estado where sigla like @sigla_estado), 
                                    @id_cf_gabinete,
                                    @cpf, 
                                    @nome_civil, 
                                    @nome_parlamentar, 
                                    @sexo, 
                                    @condicao, 
                                    @situacao, 
                                    @email, 
                                    @nascimento, 
                                    @falecimento, 
                                    (SELECT id FROM estado where sigla like @sigla_estado_nascimento), 
                                    @municipio, 
                                    @website,
                                    @escolaridade
                                )");

                                novos++;

                                sb.Append("<p>" + deputadoDetalhes.ultimoStatus.nome + "</p>");
                            }
                            else
                            {
                                banco.AddParameter("sigla_partido", deputadoDetalhes.ultimoStatus.siglaPartido);
                                banco.AddParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf);
                                banco.AddParameter("id_cf_gabinete", id_cf_gabinete);
                                banco.AddParameter("cpf", deputadoDetalhes.cpf);
                                banco.AddParameter("nome_civil", textInfo.ToTitleCase(deputadoDetalhes.nomeCivil));
                                banco.AddParameter("nome_parlamentar", textInfo.ToTitleCase(deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil));
                                banco.AddParameter("sexo", deputadoDetalhes.sexo);
                                banco.AddParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral);
                                banco.AddParameter("situacao", situacao);
                                banco.AddParameter("email", deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower());
                                banco.AddParameter("nascimento", deputadoDetalhes.dataNascimento);
                                banco.AddParameter("falecimento", deputadoDetalhes.dataFalecimento);
                                banco.AddParameter("sigla_estado_nascimento", deputadoDetalhes.ufNascimento);
                                banco.AddParameter("municipio", deputadoDetalhes.municipioNascimento);
                                banco.AddParameter("website", deputadoDetalhes.urlWebsite);
                                banco.AddParameter("escolaridade", deputadoDetalhes.escolaridade);
                                banco.AddParameter("id", deputado.id);

                                banco.ExecuteNonQuery(@"
                                    UPDATE cf_deputado SET 
                                    id_partido = (SELECT id FROM partido where sigla like @sigla_partido), 
                                    id_estado = (SELECT id FROM estado where sigla like @sigla_estado),
                                    id_cf_gabinete = @id_cf_gabinete,
                                    cpf = @cpf, 
                                    nome_civil = @nome_civil, 
                                    nome_parlamentar = @nome_parlamentar, 
                                    sexo = @sexo, 
                                    condicao = @condicao, 
                                    situacao = @situacao, 
                                    email = @email, 
                                    nascimento = @nascimento, 
                                    falecimento = @falecimento, 
                                    id_estado_nascimento = (SELECT id FROM estado where sigla like @sigla_estado_nascimento),
                                    municipio = @municipio, 
                                    website = @website,
                                    escolaridade = @escolaridade
                                    WHERE id = @id
                            ");
                            }
                        }
                    }
                } while (deputados.links.Any(x => x.rel == "next"));

            }

            if (novos > 0)
            {
                sb.Append("<p>" + novos.ToString() + " +Deputados" + "</p>");
            }

            //DownloadFotosDeputados(sDeputadosImagesPath);

            return sb.ToString();
        }

        #region Importação Dados CEAP

        public static string ImportarDespesasXml(string atualDir, int ano)
        {
            // http://www2.camara.leg.br/transparencia/cota-para-exercicio-da-atividade-parlamentar/dados-abertos-cota-parlamentar
            // http://www.camara.gov.br/cotas/AnosAnteriores.zip
            // http://www.camara.gov.br/cotas/AnoAnterior.zip
            // http://www.camara.gov.br/cotas/AnoAtual.zip

            string arquivo;
            if (ano == DateTime.Now.Year)
            {
                arquivo = "AnoAtual";
            }
            else if (ano == DateTime.Now.Year - 1)
            {
                arquivo = "AnoAnterior";
            }
            else
            {
                arquivo = "AnosAnteriores";
            }

            var downloadUrl = "http://www.camara.leg.br/cotas/" + arquivo + ".zip";
            var fullFileNameZip = atualDir + @"\" + arquivo + ".zip";
            var fullFileNameXml = atualDir + @"\" + arquivo + ".xml";

            if (!Directory.Exists(atualDir))
                Directory.CreateDirectory(atualDir);

            var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

            request.UserAgent = "Other";
            request.Method = "HEAD";
            request.ContentType = "application/json;charset=UTF-8";
            request.Timeout = 1000000;

            using (var resp = request.GetResponse())
            {
                if (File.Exists(fullFileNameZip))
                {
                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                    var ContentLengthLocal = new FileInfo(fullFileNameZip).Length;
                    if (ContentLength == ContentLengthLocal)
                    {
                        using (var banco = new Banco())
                        {
                            banco.ExecuteNonQuery(@"
								UPDATE parametros SET cf_deputado_ultima_atualizacao=NOW();
							");
                        }

                        Console.WriteLine("Não há novos itens para importar!");
                        return "<p>Não há novos itens para importar!</p>";
                    }
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");
                    client.DownloadFile(downloadUrl, fullFileNameZip);
                }
            }

            ZipFile file = null;

            try
            {
                file = new ZipFile(fullFileNameZip);

                if (file.TestArchive(true) == false)
                    throw new BusinessException("Camara: Arquivo Zip corrompido.");
            }
            finally
            {
                if (file != null)
                    file.Close();
            }

            var zip = new FastZip();
            zip.ExtractZip(fullFileNameZip, atualDir, null);

            string resumoImportacao = CarregaDadosXml(fullFileNameXml, ano);

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(@"
					UPDATE parametros SET cf_deputado_ultima_atualizacao=NOW();
				");
            }

            return resumoImportacao;

            //File.Delete(fullFileNameZip);
            //File.Delete(fullFileNameXml);
        }

        private static string CarregaDadosXml(string fullFileNameXml, int ano)
        {
            int linha = 0, inserido = 0;
            string sResumoValores = string.Empty;
            var sb = new StringBuilder();
            var linhaAtual = 0;

            // Controle, estão vindo itens duplicados no XML
            var lstHash = new Dictionary<string, long>();

            // Controle, lista para remover caso não constem no XML
            var lstHashExcluir = new Dictionary<string, long>();

            try
            {
                using (var banco = new Banco())
                {

                    using (var dt = banco.GetTable("select id, hash from cf_despesa where ano=" + ano.ToString(), 300))
                    {
                        foreach (DataRow dReader in dt.Rows)
                        {
                            try
                            {
                                lstHash.Add(dReader["hash"].ToString(), Convert.ToInt64(dReader["id"]));
                                lstHashExcluir.Add(dReader["hash"].ToString(), Convert.ToInt64(dReader["id"]));
                            }
                            catch (Exception ex)
                            {
                                sb.Append("Erro: " + ex.Message);
                            }
                            
                        }
                    }

                    using (var dReader = banco.ExecuteReader("select sum(valor_liquido) as valor, count(1) as itens from cf_despesa where ano=" + ano))
                    {
                        if (dReader.Read())
                        {
                            sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                        }
                    }

                    LimpaDespesaTemporaria(banco);

                    int vazia = 0;

                    try
                    {
                        //if (fullFileNameXml.EndsWith("AnoAnterior.xml"))
                        //    stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding(850)); //"ISO-8859-1"
                        //else
                        using (StreamReader stream = new StreamReader(fullFileNameXml, Encoding.UTF8))
                        {

                            var sqlFields = new StringBuilder();
                            var sqlValues = new StringBuilder();

                            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true }))
                            {
                                //reader.ReadToDescendant("DESPESAS");
                                //reader.ReadToDescendant("DESPESA");
                                reader.ReadToDescendant("xml");
                                reader.ReadToDescendant("dados");
                                reader.ReadToDescendant("despesa");

                                do
                                {
                                    linha++;
                                    var strXmlNodeDespeza = reader.ReadOuterXml();
                                    if (string.IsNullOrEmpty(strXmlNodeDespeza))
                                    {
                                        vazia++;
                                        if (vazia < 100)
                                        {
                                            Console.WriteLine("[vazio]" + vazia);
                                            continue;
                                        }
                                        Console.WriteLine(linhaAtual);
                                        break;
                                    }
                                    vazia = 0;

                                    var doc = new XmlDocument();
                                    doc.LoadXml(strXmlNodeDespeza);
                                    var files = doc.DocumentElement.SelectNodes("*");

                                    sqlFields.Clear();
                                    sqlValues.Clear();

                                    foreach (XmlNode fileNode in files)
                                    {
                                        if (sqlFields.Length > 0)
                                        {
                                            sqlFields.Append(",");
                                            sqlValues.Append(",");
                                        }

                                        sqlFields.Append(fileNode.Name);
                                        sqlValues.Append("@" + fileNode.Name);

                                        string value;
                                        if (fileNode.Name == "dataEmissao")
                                            value = string.IsNullOrEmpty(fileNode.InnerText) ? null : DateTime.Parse(fileNode.InnerText).ToString("yyyy-MM-dd");
                                        else if (fileNode.Name == "cnpjCPF")
                                            value = new System.Text.RegularExpressions.Regex(@"[^\d]").Replace(fileNode.InnerText.ToUpper().Trim(), "");
                                        else if (fileNode.Name == "siglaPartido")
                                            value = string.IsNullOrEmpty(fileNode.InnerText) ? null : fileNode.InnerText.ToUpper().Trim().Replace("SOLIDARIEDADE", "SD");
                                        else
                                            value = string.IsNullOrEmpty(fileNode.InnerText) ? null : fileNode.InnerText.ToUpper().Trim();

                                        banco.AddParameter(fileNode.Name, value);
                                    }

                                    string hash = banco.ParametersHash();
                                    if (lstHash.ContainsKey(hash))
                                    {
                                        lstHashExcluir.Remove(hash);
                                        banco.ClearParameters();
                                        continue;
                                    }

                                    banco.AddParameter("hash", hash);
                                    sqlFields.Append(", hash");
                                    sqlValues.Append(", @hash");

                                    try
                                    {
                                        banco.ExecuteNonQuery("INSERT INTO cf_despesa_temp (" + sqlFields + ") VALUES (" + sqlValues + ")");
                                    }
                                    catch (Exception e)
                                    {
                                        banco.ExecuteNonQuery("INSERT INTO cf_despesa_temp (" + sqlFields + ") VALUES (" + sqlValues + ")");
                                        Console.WriteLine(e.Message);
                                    }

                                    lstHash.Add(hash, 0);
                                    inserido++;

                                    if (++linhaAtual % 1000 == 0)
                                    {
                                        sb.Append(ProcessarDespesasTemp(banco));
                                        Console.WriteLine(linhaAtual);
                                    }
                                } while (true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.Append("Erro: " + ex.Message + "<br/>" + ex.StackTrace + "<br/>Hash:" + banco.ParametersHash());
                    }
                }

                using (var banco = new Banco())
                {

                    if (lstHashExcluir.Count > 0)
                    {
                        string lstExcluir = lstHashExcluir.Aggregate("", (keyString, pair) => keyString + "," + pair.Value);
                        banco.ExecuteNonQuery(string.Format("delete from cf_despesa where id IN({0})", lstExcluir.Substring(1)));
                        //banco.ExecuteNonQuery(string.Format("update cf_despesa valor_glosa=valor_documento, valor_liquido=0, exclusao=now() where id IN({0})", lstExcluir.Substring(1)));

                        Console.WriteLine("Registros para exluir: " + lstHashExcluir.Count);
                        sb.AppendFormat("<p>{0} registros excluidos</p>", lstHashExcluir.Count);
                    }
                    else if (inserido == 0)
                    {
                        sb.Append("<p>Não há novos itens para importar! #2</p>");
                        return sb.ToString();
                    }

                    if (linhaAtual > 0)
                    {
                        sb.Append(ProcessarDespesasTemp(banco));
                    }

                    using (var dReader = banco.ExecuteReader("select sum(valor_liquido) as valor, count(1) as itens from cf_despesa where ano=" + ano))
                    {
                        if (dReader.Read())
                        {
                            sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                        }
                    }

                    sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
                }

                if (ano == DateTime.Now.Year)
                {
                    AtualizaDeputadoValores();
                    AtualizaCampeoesGastos();
                    AtualizaResumoMensal();

                    using (var banco = new Banco())
                    {
                        banco.ExecuteNonQuery("call insereDados()");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append("Erro: " + ex.Message + "<br/>" + ex.StackTrace);

                // Excluir o arquivo para tentar importar novamente na proxima execução
                File.Delete(fullFileNameXml.Replace(".xml", ".zip"));
                File.Delete(fullFileNameXml);
            }

            return sb.ToString();
        }

        ///// <summary>
        ///// Baixa e Importa os Dados da CEAP
        ///// </summary>
        ///// <param name="atualDir"></param>
        ///// <param name="ano"></param>
        ///// <param name="completo"></param>
        //public static void ImportarDespesas(string atualDir, int ano, bool completo)
        //{
        //    var downloadUrl = "http://www.camara.leg.br/cotas/Ano-" + ano + ".csv.zip";
        //    var fullFileNameZip = atualDir + @"\Ano-" + ano + ".csv.zip";
        //    var fullFileNameCsv = atualDir + @"\Ano-" + ano + ".csv";

        //    if (!Directory.Exists(atualDir))
        //        Directory.CreateDirectory(atualDir);

        //    var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

        //    request.UserAgent = "Other";
        //    request.Method = "HEAD";
        //    request.ContentType = "application/json;charset=UTF-8";
        //    request.Timeout = 1000000;

        //    using (var resp = request.GetResponse())
        //    {
        //        if (File.Exists(fullFileNameZip))
        //        {
        //            long contentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
        //            long contentLengthLocal = new FileInfo(fullFileNameZip).Length;
        //            if (contentLength == contentLengthLocal)
        //            {
        //                CarregaDadosCsv(fullFileNameCsv, ano, completo);
        //                return;
        //            }
        //        }

        //        using (var client = new WebClient())
        //        {
        //            client.Headers.Add("User-Agent: Other");
        //            client.DownloadFile(downloadUrl, fullFileNameZip);
        //        }
        //    }

        //    using (ZipFile file = new ZipFile(fullFileNameZip))
        //    {
        //        if (file.TestArchive(true) == false)
        //            throw new Exception("Erro no Zip da Câmara");
        //    }

        //    var zip = new FastZip();
        //    zip.ExtractZip(fullFileNameZip, atualDir, null);

        //    CarregaDadosCsv(fullFileNameCsv, ano, completo);

        //    //File.Delete(fullFileNameZip);
        //    //File.Delete(fullFileNameXml);
        //}

       // private static void CarregaDadosCsv(string file, int ano, bool completo)
       // {
       //     //var linhaAtual = 0;
       //     var totalColunas = ColunasCEAP.Length;

       //     using (var banco = new Banco())
       //     {
       //         //if (!completo)
       //         //{
       //         //	banco.ExecuteNonQuery(@"
       //         //		DELETE FROM cf_despesa where ano=" + DateTime.Now.Year + @";

       //         //		-- select max(id)+1 from cf_despesa
       //         //		ALTER TABLE cf_despesa AUTO_INCREMENT = 2974353;
       //         //              ");
       //         //}
       //         var dt = banco.GetTable("select id, hash from cf_despesa where ano=" + ano);

       //         LimpaDespesaTemporaria(banco);

       //         using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
       //         {
       //             int count = 0;

       //             while (!reader.EndOfStream)
       //             {
       //                 count++;

       //                 var linha = reader.ReadLine();
       //                 if (string.IsNullOrEmpty(linha))
       //                     continue;

       //                 List<string> valores;
       //                 if (count == 1)
       //                 {
       //                     valores = linha.Split(';').ToList();

       //                     for (int i = 0; i < totalColunas - 1; i++)
       //                     {
       //                         if (valores[i] != ColunasCEAP[i])
       //                         {
       //                             throw new Exception("Mudança de integração detectada para o Câmara Federal");
       //                         }
       //                     }

       //                     // Pular linha de titulo
       //                     continue;
       //                 }

       //                 valores = linha.Split(new[] { ";" }, StringSplitOptions.None).ToList();
       //                 for (int i = 0; i < totalColunas; i++)
       //                 {
       //                     if (ColunasCEAP[i] == "dataEmissao")
       //                     {
       //                         banco.AddParameter(ColunasCEAP[i], string.IsNullOrEmpty(valores[i]) ? null : Convert.ToDateTime(valores[i]).ToString("yyyy-MM-dd"));
       //                     }
       //                     else if (!ColunasCEAP[i].StartsWith("tx") && !ColunasCEAP[i].StartsWith("sg"))
       //                     {
       //                         if (ColunasCEAP[i] == "valorLiquido" && valores[i] == "-0")
       //                         {
       //                             // Valor liquido deve ser calculado pelo valorDocumento-valorGlosa-restituicao
       //                             banco.AddParameter(ColunasCEAP[i], null);
       //                         }
       //                         else
       //                         {
       //                             //OBS: Necessario dar replace para o caso do restituicao que esta vindo com o separador trocado.
       //                             banco.AddParameter(ColunasCEAP[i], string.IsNullOrEmpty(valores[i]) ? null : (object)Convert.ToDecimal(valores[i].Replace(".", ",")));
       //                         }
       //                     }
       //                     else
       //                     {
       //                         banco.AddParameter(ColunasCEAP[i], valores[i].ToUpper());
       //                     }
       //                 }

       //                 string hash = banco.ParametersHash();

       //                 var lstDr = dt.Select("hash='" + hash + "'");
       //                 if (lstDr.Length > 0)
       //                 {
       //                     dt.Rows.Remove(lstDr[0]);
       //                     banco.ClearParameters();
       //                     continue;
       //                 }

       //                 banco.AddParameter("hash", hash);

       //                 banco.ExecuteNonQuery(
       //                     @"INSERT INTO cf_despesa_temp (
							//	txNomeParlamentar, numeroDeputadoID, numeroCarteiraParlamentar, legislatura, siglaUF, 
							//	siglaPartido, codigoLegislatura, numeroSubCota, txtDescricao, numeroEspecificacaoSubCota, 
							//	txtDescricaoEspecificacao, fornecedor, cnpjCPF, numero, tipoDocumento, 
							//	dataEmissao, valorDocumento, valorGlosa, valorLiquido, mes, 
							//	ano, parcela, passageiro, trecho, lote, 
							//	ressarcimento, restituicao, numeroDeputadoID, idDocumento, hash
							//) VALUES (
							//	@txNomeParlamentar, @numeroDeputadoID, @numeroCarteiraParlamentar, @legislatura, @siglaUF, 
							//	@siglaPartido, @codigoLegislatura, @numeroSubCota, @txtDescricao, @numeroEspecificacaoSubCota, 
							//	@txtDescricaoEspecificacao, @fornecedor, @cnpjCPF, @numero, @tipoDocumento, 
							//	@dataEmissao, @valorDocumento, @valorGlosa, @valorLiquido, @mes, 
							//	@ano, @parcela, @passageiro, @trecho, @lote, 
							//	@ressarcimento, @restituicao, @numeroDeputadoID, @idDocumento, @hash
							//)");

       //                 //if (++linhaAtual == 10000)
       //                 //{
       //                 //	linhaAtual = 0;

       //                 //	ProcessarDespesasTemp(banco, true);
       //                 //}
       //             }
       //         }

       //         ProcessarDespesasTemp(banco);

       //         foreach (DataRow dr in dt.Rows)
       //         {
       //             banco.AddParameter("id", dr["id"].ToString());
       //             banco.ExecuteNonQuery("delete from sf_despesa where id=@id");
       //         }
       //     }

       //     if (ano == DateTime.Now.Year)
       //     {
       //         AtualizaDeputadoValores();

       //         AtualizaCampeoesGastos();

       //         AtualizaResumoMensal();

       //         using (var banco = new Banco())
       //         {
       //             banco.ExecuteNonQuery(@"
				   //     UPDATE parametros SET cf_deputado_ultima_atualizacao=NOW();
			    //    ");
       //         }
       //     }
       // }

        private static string ProcessarDespesasTemp(Banco banco)
        {
            var sb = new StringBuilder();

            CorrigeDespesas(banco);
            sb.Append(InsereDeputadoFaltante(banco));
            //sb.Append(InsereTipoDespesaFaltante(banco));
            //sb.Append(InsereTipoEspecificacaoFaltante(banco));
            sb.Append(InsereMandatoFaltante(banco));
            sb.Append(InsereLegislaturaFaltante(banco));
            sb.Append(InsereFornecedorFaltante(banco));


            //if (completo)
            sb.Append(InsereDespesaFinal(banco));
            //else
            //	InsereDespesaFinalParcial(banco);

            LimpaDespesaTemporaria(banco);

            return sb.ToString();
        }

        private static void CorrigeDespesas(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				UPDATE cf_despesa_temp SET numero = NULL WHERE numero = 'S/N' OR numero = '';
				UPDATE cf_despesa_temp SET numeroDeputadoID = NULL WHERE numeroDeputadoID = '';
				UPDATE cf_despesa_temp SET cnpjCPF = NULL WHERE cnpjCPF = '';
				UPDATE cf_despesa_temp SET fornecedor = 'CORREIOS' WHERE cnpjCPF is null and fornecedor LIKE 'CORREIOS%';
				-- UPDATE cf_despesa_temp set valorLiquido = valorDocumento - valorGlosa - restituicao where valorLiquido is null;

                UPDATE cf_despesa_temp SET siglaUF = NULL WHERE siglaUF = 'NA';
                UPDATE cf_despesa_temp SET numeroEspecificacaoSubCota = NULL WHERE numeroEspecificacaoSubCota = 0;
                UPDATE cf_despesa_temp SET lote = NULL WHERE lote = 0;
                UPDATE cf_despesa_temp SET ressarcimento = NULL WHERE ressarcimento = 0;
                UPDATE cf_despesa_temp SET idDocumento = NULL WHERE idDocumento = 0;
                UPDATE cf_despesa_temp SET parcela = NULL WHERE parcela = 0;
			");
        }

        private static string InsereDeputadoFaltante(Banco banco)
        {
            // Atualiza os deputados já existentes quando efetuarem os primeiros gastos com a cota
            banco.ExecuteNonQuery(@"
                UPDATE cf_deputado d
                inner join cf_despesa_temp dt on d.id = dt.idDeputado
                set d.id_deputado = dt.numeroDeputadoID
                where d.id_deputado is null;
            ");

            // Insere um novo deputado ou liderança
            banco.ExecuteNonQuery(@"
        	    INSERT INTO cf_deputado (id, id_deputado, nome_parlamentar)
        	    select distinct idDeputado, numeroDeputadoID, nomeParlamentar 
        	    from cf_despesa_temp
        	    where numeroDeputadoID  not in (
        		    select id_deputado from cf_deputado
        	    );
            ");

            //DownloadFotosDeputados(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

            if (banco.Rows > 0)
            {
                //AtualizaInfoDeputados();
                //AtualizaInfoDeputadosCompleto();

                return "<p>" + banco.Rows + "+ Deputado<p>";
            }

            return string.Empty;
        }

        private static string InsereTipoDespesaFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
        	INSERT INTO cf_despesa_tipo (id, descricao)
        	select distinct numeroSubCota, descricao
        	from cf_despesa_temp
        	where numeroSubCota  not in (
        		select id from cf_despesa_tipo
        	);
        ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Tipo Despesa<p>";
            }

            return string.Empty;
        }

        private static string InsereTipoEspecificacaoFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
            	INSERT INTO cf_especificacao_tipo (id_cf_despesa_tipo, id_cf_especificacao, descricao)
            	select distinct numeroSubCota, numeroEspecificacaoSubCota, descricaoEspecificacao
            	from cf_despesa_temp dt
            	left join cf_especificacao_tipo tp on tp.id_cf_despesa_tipo = dt.numeroSubCota 
            		and tp.id_cf_especificacao = dt.numeroEspecificacaoSubCota
            	where numeroEspecificacaoSubCota <> 0
            	AND tp.descricao = null;
            ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Tipo Especificação<p>";
            }

            return string.Empty;
        }

        private static string InsereMandatoFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
                SET SQL_BIG_SELECTS=1;

				INSERT INTO cf_mandato (id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido)
				select distinct dt.numeroDeputadoID, codigoLegislatura, numeroCarteiraParlamentar, e.id, p.id 
				from ( 
					select distinct 
					numeroDeputadoID, legislatura, numeroCarteiraParlamentar, codigoLegislatura, siglaUF, siglaPartido
					from cf_despesa_temp
				) dt
				left join estado e on e.sigla = dt.siglaUF
				left join partido p on p.sigla = dt.siglaPartido
				left join cf_mandato m on m.id_cf_deputado = dt.numeroDeputadoID 
					AND m.id_legislatura = dt.codigoLegislatura 
					AND m.id_carteira_parlamantar = numeroCarteiraParlamentar
				where dt.codigoLegislatura <> 0
				AND m.id is null;

                SET SQL_BIG_SELECTS=0;
			");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Mandato<p>";
            }

            return string.Empty;
        }

        private static string InsereLegislaturaFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				INSERT INTO cf_legislatura (id, ano)
				select distinct codigoLegislatura, legislatura
				from cf_despesa_temp dt
				where codigoLegislatura <> 0
				AND codigoLegislatura  not in (
					select id from cf_legislatura
				);
			");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Legislatura<p>";
            }

            return string.Empty;
        }

        private static string InsereFornecedorFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
                SET SQL_BIG_SELECTS=1;

				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.fornecedor), dt.cnpjCPF
				from cf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.cnpjCPF
				where dt.cnpjCPF is not null
				and f.id is null
				GROUP BY dt.cnpjCPF;

				INSERT INTO fornecedor (nome, cnpj_cpf)
				select DISTINCT dt.fornecedor, dt.cnpjCPF
				from cf_despesa_temp dt
				left join fornecedor f on f.nome = dt.fornecedor
				where dt.cnpjCPF is null
				and f.id is null;

                SET SQL_BIG_SELECTS=0;
			");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Fornecedor<p>";
            }

            return string.Empty;
        }

        private static string InsereDespesaFinal(Banco banco)
        {
            banco.ExecuteNonQuery(@"
                SET SQL_BIG_SELECTS=1;
				 ALTER TABLE cf_despesa DISABLE KEYS;

				INSERT INTO cf_despesa (
					id_documento,
					id_cf_deputado,
					id_cf_mandato,
					id_cf_despesa_tipo,
					id_cf_especificacao,
					id_fornecedor,
					nome_passageiro,
					numero_documento,
					tipo_documento,
					data_emissao,
					valor_documento,
					valor_glosa,
					valor_liquido,
					valor_restituicao,
					mes,
					ano,
					parcela,
					trecho_viagem,
					lote,
					ressarcimento,
					ano_mes,
					hash,
					importacao
				)
				select 
					dt.idDocumento,
					d.id,
					m.id,
					numeroSubCota,
					numeroEspecificacaoSubCota,
					f.id,
					passageiro, -- id_passageiro,
					numero,
					tipoDocumento,
					dataEmissao,
					valorDocumento,
					valorGlosa,
					valorLiquido,
					restituicao,
					mes,
					ano,
					parcela,
					trecho,
					lote,
					ressarcimento,
					concat(ano, LPAD(mes, 2, '0')),
					hash,
					now()
				from cf_despesa_temp dt
                LEFT JOIN cf_deputado d on d.id_deputado = dt.numeroDeputadoID
				LEFT join fornecedor f on f.cnpj_cpf = dt.cnpjCPF
					or (f.cnpj_cpf is null and dt.cnpjCPF is null and f.nome = dt.fornecedor)
				left join cf_mandato m on m.id_cf_deputado = d.id
					and m.id_legislatura = dt.codigoLegislatura 
					and m.id_carteira_parlamantar = numeroCarteiraParlamentar;
    
				ALTER TABLE cf_despesa ENABLE KEYS;
                SET SQL_BIG_SELECTS=0;
			", 3600);

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Despesa<p>";
            }

            return string.Empty;
        }

       // private static void InsereDespesaFinalParcial(Banco banco)
       // {
       //     var dt = banco.GetTable(
       //         @"SET SQL_BIG_SELECTS=1;

       //         DROP TABLE IF EXISTS table_in_memory_d;
       //         CREATE TEMPORARY TABLE table_in_memory_d
       //         AS (
	      //          select id_cf_deputado, mes, sum(valor_liquido) as total
	      //          from cf_despesa d
	      //          where ano = 2017
	      //          GROUP BY id_cf_deputado, mes
       //         );

       //         DROP TABLE IF EXISTS table_in_memory_dt;
       //         CREATE TEMPORARY TABLE table_in_memory_dt
       //         AS (
	      //          select numeroDeputadoID, mes, sum(valorLiquido) as total
	      //          from cf_despesa_temp d
	      //          GROUP BY numeroDeputadoID, mes
       //         );

       //         select dt.numeroDeputadoID as id_cf_deputado, dt.mes as mes
       //         from table_in_memory_dt dt
       //         left join table_in_memory_d d on dt.numeroDeputadoID = d.id_cf_deputado and dt.mes = d.mes
       //         where (d.id_cf_deputado or d.total <> dt.total)
       //         order by d.id_cf_deputado, d.mes;

       //         SET SQL_BIG_SELECTS=0;
			    //", 3600);

       //     foreach (DataRow dr in dt.Rows)
       //     {
       //         banco.AddParameter("id_cf_deputado", dr["id_cf_deputado"]);
       //         banco.AddParameter("mes", dr["mes"]);
       //         banco.ExecuteNonQuery(
       //             @"DELETE FROM cf_despesa WHERE id_cf_deputado=@id_cf_deputado and ano=2017 and mes=@mes");

       //         banco.AddParameter("id_cf_deputado", dr["id_cf_deputado"]);
       //         banco.AddParameter("mes", dr["mes"]);
       //         banco.ExecuteNonQuery(@"
       //                 SET SQL_BIG_SELECTS=1;

				   //     INSERT INTO cf_despesa (
					  //      id_documento,
					  //      id_cf_deputado,
					  //      id_cf_mandato,
					  //      id_cf_despesa_tipo,
					  //      id_cf_especificacao,
					  //      id_fornecedor,
					  //      nome_passageiro,
					  //      numero_documento,
					  //      tipo_documento,
					  //      data_emissao,
					  //      valor_documento,
					  //      valor_glosa,
					  //      valor_liquido,
					  //      valor_restituicao,
					  //      mes,
					  //      ano,
					  //      parcela,
					  //      trecho_viagem,
					  //      lote,
					  //      ressarcimento,
					  //      ano_mes
				   //     )
				   //     select 
						 //   dt.idDocumento,
						 //   dt.numeroDeputadoID,
						 //   m.id,
						 //   numeroSubCota,
						 //   numeroEspecificacaoSubCota,
						 //   f.id,
						 //   passageiro, -- id_passageiro,
						 //   numero,
						 //   tipoDocumento,
						 //   dataEmissao,
						 //   valorDocumento,
						 //   valorGlosa,
						 //   valorLiquido,
						 //   restituicao,
						 //   mes,
						 //   ano,
						 //   parcela,
						 //   trecho,
						 //   lote,
						 //   ressarcimento,
						 //   concat(ano, LPAD(mes, 2, '0'))
					  //  from (
						 //   select *					        
						 //   from cf_despesa_temp dt
						 //   WHERE numeroDeputadoID=@id_cf_deputado and mes=@mes
					  //  ) dt
				   //     LEFT join fornecedor f on f.cnpj_cpf = dt.cnpjCPF
					  //      or (f.cnpj_cpf is null and dt.cnpjCPF is null and f.nome = dt.fornecedor)
				   //     left join cf_mandato m on m.id_cf_deputado = dt.numeroDeputadoID
					  //      and m.id_legislatura = dt.codigoLegislatura 
					  //      and m.id_carteira_parlamantar = numeroCarteiraParlamentar;

       //                 SET SQL_BIG_SELECTS=0;
			    //    ", 3600);
       //     }
       // }

        private static void LimpaDespesaTemporaria(Banco banco)
        {
            banco.ExecuteNonQuery(@"

                truncate table cf_despesa_temp;
			");
        }

        #endregion Importação Dados CEAP

        public static void AtualizaDeputadoValores()
        {
            using (var banco = new Banco())
            {
                var dt = banco.GetTable("select id from cf_deputado");
                object quantidade_secretarios;
                object valor_total_ceap;

                foreach (DataRow dr in dt.Rows)
                {
                    banco.AddParameter("id_cf_deputado", dr["id"]);
                    valor_total_ceap = banco.ExecuteScalar("select sum(valor_liquido) from cf_despesa where id_cf_deputado=@id_cf_deputado;");

                    banco.AddParameter("id_cf_deputado", dr["id"]);
                    quantidade_secretarios = banco.ExecuteScalar("select count(1) from cf_secretario where id_cf_deputado=@id_cf_deputado;");

                    banco.AddParameter("quantidade_secretarios", quantidade_secretarios);
                    banco.AddParameter("valor_total_ceap", valor_total_ceap);
                    banco.AddParameter("id_cf_deputado", dr["id"]);
                    banco.ExecuteNonQuery(
                        @"update cf_deputado set 
						quantidade_secretarios=@quantidade_secretarios
						, valor_total_ceap=@valor_total_ceap 
						where id=@id_cf_deputado"
                    );
                }
            }
        }

        /// <summary>
        /// Atualiza indicador 'Campeões de gastos',
        /// Os 4 deputados que mais gastaram com a CEAP desde o ínicio do mandato 55 (02/2015)
        /// </summary>
        public static void AtualizaCampeoesGastos()
        {
            var strSql =
                @"truncate table cf_deputado_campeao_gasto;
				insert into cf_deputado_campeao_gasto
				SELECT l1.id_cf_deputado, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_cf_deputado,
						sum(l.valor_liquido) as valor_total
					FROM  cf_despesa l
					where l.ano_mes >= 201902 
					GROUP BY l.id_cf_deputado
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado 
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;";

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        public static void AtualizaResumoMensal()
        {
            var strSql =
                @"truncate table cf_despesa_resumo_mensal;
				insert into cf_despesa_resumo_mensal
				(ano, mes, valor) (
					select ano, mes, sum(valor_liquido)
					from cf_despesa
					group by ano, mes
				);";

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        /// <summary>
        /// Baixa as imagens dos deputados novos (imagens que ainda não foram baixadas)
        /// </summary>
        /// <param name="dirRaiz"></param>
        public static string DownloadFotosDeputados(string dirRaiz, bool cargaInicial = false)
        {
            var sb = new StringBuilder();
            using (var banco = new Banco())
            {
                string sql = "SELECT id FROM cf_deputado where id_deputado is not null";
                if (!cargaInicial)
                {
                    sql += " AND situacao <> 'Fim de Mandato'";
                }
                DataTable table = banco.GetTable(sql, 0);

                foreach (DataRow row in table.Rows)
                {
                    string id = row["id"].ToString();
                    string src = dirRaiz + id + ".jpg";
                    if (File.Exists(src)) continue;

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add("User-Agent: Other");
                            client.DownloadFile("http://www.camara.gov.br/internet/deputado/bandep/" + id + ".jpg", src);

                            ImportacaoUtils.CreateImageThumbnail(src);
                        }
                    }
                    catch (Exception)
                    {
                        sb.AppendLine("Imagem do deputado " + id + " inexistente!");
                    }
                }
            }

            return sb.ToString();
        }
    }


    public class Dado
    {
        public int id { get; set; }
        public int idLegislatura { get; set; }
        public string nome { get; set; }
        public string siglaPartido { get; set; }
        public string siglaUf { get; set; }
        public string uri { get; set; }
        public string uriPartido { get; set; }
        public string urlFoto { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
        public string rel { get; set; }
    }

    public class Deputados
    {
        public List<Dado> dados { get; set; }
        public List<Link> links { get; set; }
    }

    public class Gabinete
    {
        public string andar { get; set; }
        public string email { get; set; }
        public string nome { get; set; }
        public string predio { get; set; }
        public string sala { get; set; }
        public string telefone { get; set; }
    }

    public class UltimoStatus
    {
        public string condicaoEleitoral { get; set; }
        public DateTime data { get; set; }
        public string descricaoStatus { get; set; }
        public Gabinete gabinete { get; set; }
        public int id { get; set; }
        public int idLegislatura { get; set; }
        public string nome { get; set; }
        public string nomeEleitoral { get; set; }
        public string siglaPartido { get; set; }
        public string siglaUf { get; set; }
        public string situacao { get; set; }
        public string uri { get; set; }
        public string uriPartido { get; set; }
        public string urlFoto { get; set; }
    }

    public class Dados
    {
        public string cpf { get; set; }
        public DateTime? dataFalecimento { get; set; }
        public DateTime dataNascimento { get; set; }
        public string escolaridade { get; set; }
        public int id { get; set; }
        public string municipioNascimento { get; set; }
        public string nomeCivil { get; set; }
        public List<string> redeSocial { get; set; }
        public string sexo { get; set; }
        public string ufNascimento { get; set; }
        public UltimoStatus ultimoStatus { get; set; }
        public string uri { get; set; }
        public string urlWebsite { get; set; }
    }

    public class DeputadoDetalhes
    {
        public Dados dados { get; set; }
        public List<Link> links { get; set; }
    }
}