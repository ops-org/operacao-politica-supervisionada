using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OPS.Core;
using OPS.ImportacaoDados;
using System.Xml;
using System.Data;
using System.Net;

namespace OPS.ImportacaoManual
{
    internal class Program
    {
        //public static void ConverterXmlParaCsvDespesasCamara(string tempPath)
        //{
        //	string[] ColunasXmlDespesasCamara = new string[]
        //   {
        //	"txNomeParlamentar",
        //	"ideCadastro",
        //	"nuCarteiraParlamentar",
        //	"nuLegislatura",
        //	"sgUF",
        //	"sgPartido",
        //	"codLegislatura",
        //	"numSubCota",
        //	"txtDescricao",
        //	"numEspecificacaoSubCota",
        //	"txtDescricaoEspecificacao",
        //	"txtFornecedor",
        //	"txtCNPJCPF",
        //	"txtNumero",
        //	"indTipoDocumento",
        //	"datEmissao",
        //	"vlrDocumento",
        //	"vlrGlosa",
        //	"vlrLiquido",
        //	"numMes",
        //	"numAno",
        //	"numParcela",
        //	"txtPassageiro",
        //	"txtTrecho",
        //	"numLote",
        //	"numRessarcimento",
        //	"ideDocumento",
        //	"vlrRestituicao",
        //	"nuDeputadoId"
        //   };

        //	string fullFileNameXml = tempPath + @"\AnoAtual.xml";
        //	StreamReader stream = null;

        //	try
        //	{
        //		if (fullFileNameXml.EndsWith("AnoAnterior.xml"))
        //			stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding(850)); //"ISO-8859-1"
        //		else
        //			stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding("ISO-8859-1"));

        //		using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreComments = true }))
        //		{
        //			reader.ReadToDescendant("DESPESAS");
        //			reader.ReadToDescendant("DESPESA");

        //			using (StreamWriter outputFile = new StreamWriter(fullFileNameXml.Replace(".xml", ".csv")))
        //			{
        //				do
        //				{
        //					var lstCsv = new List<string>();
        //					var strXmlNodeDespeza = reader.ReadOuterXml();
        //					if (string.IsNullOrEmpty(strXmlNodeDespeza))
        //						break;

        //					XmlDocument doc = new XmlDocument();
        //					doc.LoadXml(strXmlNodeDespeza);
        //					XmlNodeList files = doc.DocumentElement.SelectNodes("*");

        //					int indexXml = 0;
        //					for (int i = 0; i < 29; i++)
        //					{
        //						if (files[indexXml].Name == ColunasXmlDespesasCamara[i])
        //						{
        //							lstCsv.Add(files[indexXml++].InnerText);
        //						}
        //						else
        //						{
        //							lstCsv.Add("");
        //						}

        //					}

        //					outputFile.WriteLine(string.Join(";", lstCsv));
        //				}
        //				while (true);
        //			}

        //			reader.Close();
        //		}
        //	}
        //	catch (Exception)
        //	{
        //		throw;
        //	}
        //	finally
        //	{
        //		stream.Close();
        //		stream.Dispose();
        //	}
        //}

        public static void Main(string[] args)
        {
            Padrao.ConnectionString = ConfigurationManager.ConnectionStrings["AuditoriaContext"].ToString();

            var tempPath = @"D:\GitHub\operacao-politica-supervisionada\OPS\temp";

            ////ConverterXmlParaCsvDespesasCamara(tempPath);

            //#region Camara
            //Camara.AtualizaInfoDeputados();
            //Camara.AtualizaInfoDeputadosCompleto();

            //Camara.ImportarMandatos();
            //Camara.DownloadFotosDeputados(@"D:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

            //Importação na nova estrutura
            //for (int ano = 2009; ano <= 2018; ano++)
            //{
            //    Camara.ImportarDespesas(tempPath, ano, true);
            //}
            //Camara.ImportarDespesas(tempPath, 2017, false);

            //Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2016));
            //Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2017));
            //Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2018));
            ////Camara.AtualizaDeputadoValores();
            //Camara.ImportaPresencasDeputados();

            //Camara.AtualizaDeputadoValores();
            //Camara.AtualizaCampeoesGastos();
            //Camara.AtualizaResumoMensal();
            //Camara.ImportarMandatos();
            //Camara.ImportaPresencasDeputados();

            //Camara.AtualizaInfoDeputadosCompleto();
            //Camara.ImportarDeputados(52);

            ////Camara.ValidarLinkRecibos();
            //Camara.AtualizaResumoMensal();
            ////#endregion Camara

            ////#region Senado
            ////Senado.CarregaSenadores();
            //Senado.DownloadFotosSenadores(@"D:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\SENADOR\");
            //Senado.CarregaSenadoresAtuais();

            //for (int ano = 2008; ano <= 2017; ano++)
            //{
            //	Senado.ImportarDespesas(tempPath, ano, true);
            //}
            //Senado.ImportarDespesas(tempPath, 2017, false);
            //#endregion Senado

            //Fornecedor.ConsultarReceitaWS();

            //var s = Camara.ImportarDeputados(
            //    @"D:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

            //      Console.Write(s);

            ////Fornecedor.AtualizaFornecedorDoador();
            //Fornecedor.ConsultarCNPJ();
            //Fornecedor.ConsultarReceitaWS();
            //var s = Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false);
            //var s = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1);

            using (var banco = new Banco())
            {
                while (true)
                {
                    string sql = "SELECT id, id_cf_deputado, id_documento, ano FROM cf_despesa where id_documento is not null and link = 0 order by id desc limit 1000";
                    DataTable table = banco.GetTable(sql, 0);

                    if (table.Rows.Count > 0)
                    {
                        foreach (DataRow dr in table.Rows)
                        {
                            string downloadUrl = "http://www.camara.gov.br/cota-parlamentar/documentos/publ/" + dr["id_cf_deputado"].ToString() + "/" + dr["ano"].ToString() + "/" + dr["id_documento"].ToString() + ".pdf"; 

                            var request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                            request.UserAgent = "Other";
                            request.Method = "HEAD";
                            request.Timeout = 1000000;

                            try
                            {
                                using (var resp = request.GetResponse())
                                {
                                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                                    if (ContentLength == 0)
                                        Console.WriteLine("pdf-0:" + downloadUrl);
                                }

                                banco.AddParameter("link", 2); //PDF
                                banco.AddParameter("id", dr["id"]);
                                banco.ExecuteNonQuery("UPDATE cf_despesa SET link=? WHERE id=?");

                                continue;
                            }
                            catch (Exception ex)
                            {
                                if (!ex.Message.Contains("404"))
                                    throw;
                            }

                            if (Convert.ToInt32(dr["ano"]) >= 2017)
                            {
                                downloadUrl = "http://www.camara.gov.br/cota-parlamentar/nota-fiscal-eletronica?ideDocumentoFiscal=" + dr["id_documento"].ToString();
                                request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                                request.UserAgent = "Other";
                                request.Method = "HEAD";
                                request.Timeout = 1000000;

                                try
                                {
                                    request.GetResponse();

                                    banco.AddParameter("link", 3); //NF-e
                                    banco.AddParameter("id", dr["id"]);
                                    banco.ExecuteNonQuery("UPDATE cf_despesa SET link=? WHERE id=?");

                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    if (!ex.Message.Contains("500"))
                                        throw;
                                }
                            }

                            banco.AddParameter("link", 1); // Não existe Recibo
                            banco.AddParameter("id", dr["id"]);
                            banco.ExecuteNonQuery("UPDATE cf_despesa SET link=? WHERE id=?");
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }


            //Console.Write(s);
        }
    }
}