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
using System.Diagnostics;
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

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


            //while (true)
            //{
            //    using (var banco = new Banco())
            //    {
            //        string sql =
            //            "SELECT id, id_cf_deputado, id_documento, ano FROM cf_despesa where id_documento is not null and link is null order by id desc limit 1000";
            //        DataTable table = banco.GetTable(sql, 0);

            //        if (table.Rows.Count > 0)
            //        {

            //            foreach (DataRow dr in table.Rows)
            //            {
            //                string downloadUrl = "http://www.camara.gov.br/cota-parlamentar/documentos/publ/" +
            //                                     dr["id_cf_deputado"].ToString() + "/" + dr["ano"].ToString() + "/" +
            //                                     dr["id_documento"].ToString() + ".pdf";

            //                HttpWebRequest request = null;

            //                if (Convert.ToInt32(dr["ano"]) >= 2017)
            //                {
            //                    downloadUrl = "http://www.camara.gov.br/cota-parlamentar/nota-fiscal-eletronica?ideDocumentoFiscal=" + dr["id_documento"].ToString();
            //                    request = (HttpWebRequest)WebRequest.Create(downloadUrl);
            //                    request.UserAgent = "Other";
            //                    request.Method = "HEAD";
            //                    request.Timeout = 1000000;

            //                    try
            //                    {
            //                        request.GetResponse();

            //                        banco.AddParameter("link", 3); //NF-e
            //                        banco.AddParameter("id", dr["id"]);
            //                        banco.ExecuteNonQuery("UPDATE cf_despesa SET link=? WHERE id=?");

            //                        continue;
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        if (!ex.Message.Contains("500"))
            //                            throw;
            //                    }
            //                }

            //                //try
            //                //{
            //                //    request = (HttpWebRequest)WebRequest.Create(downloadUrl);
            //                //    request.UserAgent = "Other";
            //                //    request.Method = "HEAD";
            //                //    request.Timeout = 1000000;

            //                //    using (var resp = request.GetResponse())
            //                //    {
            //                //        var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
            //                //        if (ContentLength == 0)
            //                //            Console.WriteLine("pdf-0:" + downloadUrl);
            //                //    }

            //                //    banco.AddParameter("link", 2); //PDF
            //                //    banco.AddParameter("id", dr["id"]);
            //                //    banco.ExecuteNonQuery("UPDATE cf_despesa SET link=? WHERE id=?");

            //                //    continue;
            //                //}
            //                //catch (Exception ex)
            //                //{
            //                //    if (!ex.Message.Contains("404"))
            //                //        //throw;
            //                //        continue;
            //                //}

            //                banco.AddParameter("link", 0); // Não existe Recibo
            //                banco.AddParameter("id", dr["id"]);
            //                banco.ExecuteNonQuery("UPDATE cf_despesa SET link=? WHERE id=?");
            //            }

            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}


            //Console.Write(s);


            //Senado.CarregaSenadoresAtuais();

            //var d = Camara.ImportarDeputados();
            //var a = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1);
            //var b = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year);


            //Fornecedor.ConsultarReceitaWS();
            //Camara.ImportarDeputados();

            //var sb = new StringBuilder();
            //TimeSpan t;
            //Stopwatch sw = Stopwatch.StartNew();
            //Stopwatch swGeral = Stopwatch.StartNew();


            //(new Core.DAO.ParametrosDao()).CarregarPadroes();


            ////sb.AppendFormat("<br/><h3>-- Importar Deputados --</h3>");
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Camara.ImportarDeputados());
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);


            //sb.AppendFormat("<br/><h3>-- Importar Despesas Deputados {0} --</h3>", DateTime.Now.Year - 1);
            //sb.AppendFormat("<br/><p>" + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + "</p>");
            //sw.Restart();
            //try
            //{
            //    sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1));
            //}
            //catch (Exception ex)
            //{
            //    sb.Append(ex.Message);
            //}
            //t = sw.Elapsed;
            //sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Importar Despesas Deputados {0} --</h3>", DateTime.Now.Year);
            ////sb.AppendFormat("<br/><p>" + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + "</p>");
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year));
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Importar Presenças Deputados --</h3>");
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Camara.ImportaPresencasDeputados());
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Importar Senadores {0} --</h3>", DateTime.Now.Year);
            ////sw.Restart();
            ////try
            ////{
            ////    Senado.CarregaSenadores();
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Importar Senadores {0} --</h3>", DateTime.Now.Year);
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Senado.CarregaSenadoresAtuais());
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Importar Despesas Senado {0} --</h3>", DateTime.Now.Year - 1);
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false));
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Importar Despesas Senado {0} --</h3>", DateTime.Now.Year);
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false));
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////sb.AppendFormat("<br/><h3>-- Consultar Receita WS --</h3>");
            ////sw.Restart();
            ////try
            ////{
            ////    sb.Append(Fornecedor.ConsultarReceitaWS());
            ////}
            ////catch (Exception ex)
            ////{
            ////    sb.Append(ex.Message);
            ////}
            ////t = sw.Elapsed;
            ////sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

            ////for (int ano = 2014; ano <= 2019; ano++)
            ////{
            ////    sb.Append(CamaraDistritoFederal.ImportarDespesas(tempPath, ano, true));
            ////}

            //t = swGeral.Elapsed;
            //sb.AppendFormat("<br/><h3>Duração Total: {0:D2}h:{1:D2}m:{2:D2}s</h3>", t.Hours, t.Minutes, t.Seconds);

            //Console.WriteLine(sb.ToString());


            //tempPath = System.AppDomain.CurrentDomain.BaseDirectory;
            //using (var banco = new Banco())
            //{
            //    string sql = @"
            //        SELECT ds.id, dp.id_deputado, ds.id_documento, ds.ano 
            //        FROM cf_despesa ds
            //        INNER JOIN cf_deputado dp on dp.id = ds.id_cf_deputado
            //        WHERE ds.ano >= 2015
            //        and ds.id_fornecedor IN(78823, 89903, 102626) 
            //        and id_documento is not null
            //        order by ds.id DESC";

            //    DataTable table = banco.GetTable(sql, 0);

            //    if (table.Rows.Count > 0)
            //    {
            //        Console.WriteLine("Total: " + table.Rows.Count);

            //        var ilegivel = 0;
            //        foreach (DataRow dr in table.Rows)
            //        {
            //            string downloadUrl = "http://www.camara.gov.br/cota-parlamentar/documentos/publ/" + dr["id_deputado"].ToString() + "/" + dr["ano"].ToString() + "/" + dr["id_documento"].ToString() + ".pdf";
            //            string fileName = tempPath + "\\pdf\\" + dr["id"].ToString() + ".pdf";
            //            string fileNameOK = tempPath + "\\pdf_ok\\" + dr["id"].ToString() + ".pdf";
            //            string fileNameSuspeito = tempPath + "\\pdf_suspeito\\" + dr["id"].ToString() + ".pdf";

            //            if (!File.Exists(fileName) && !File.Exists(fileNameOK) && !File.Exists(fileNameSuspeito))
            //            {
            //                try
            //                {
            //                    using (var client = new WebClient())
            //                    {
            //                        client.Headers.Add("User-Agent: Other");
            //                        client.DownloadFile(downloadUrl, fileName);
            //                    }
            //                }
            //                catch (WebException e)
            //                {
            //                    if (e.Message.Contains("404")) continue;

            //                    Console.WriteLine(e);
            //                    throw;
            //                }
            //            }

            //            try
            //            {
            //                var lstUber = ReadPdfFile(fileName, "Uber");
            //                if (!lstUber.Any())
            //                {
            //                    ilegivel++; //Console.WriteLine("Ilegivel:" + dr["id"].ToString() + " - " + downloadUrl);
            //                    continue;
            //                }

            //                var lstExtra = ReadPdfFile(fileName, "Extra");
            //                if (lstExtra.Any())
            //                {
            //                    File.Move(fileName, fileNameSuspeito);
            //                    //Console.WriteLine(dr["id"].ToString() + " - " + downloadUrl);
            //                    continue;
            //                }

            //                File.Move(fileName, fileNameOK);
            //            }
            //            catch (Exception e)
            //            {
            //                //Console.WriteLine(e.Message);
            //            }
            //        }

            //        Console.WriteLine("Ilegivel: " + ilegivel);
            //    }
            //}


            Console.WriteLine("Concluido! Tecle [ENTER] para sair.");
            Console.ReadKey();


        }

        public static List<int> ReadPdfFile(string fileName, String searthText)
        {
            List<int> pages = new List<int>();
            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);
                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    if (currentPageText.Contains(searthText))
                    {
                        pages.Add(page);
                    }
                }
                pdfReader.Close();
            }
            return pages;
        }
    }
}