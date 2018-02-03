using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OPS.Core;
using OPS.ImportacaoDados;
using System.Xml;

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

			var tempPath = @"D:\temp";

			////ConverterXmlParaCsvDespesasCamara(tempPath);

			//#region Camara
			//Camara.AtualizaInfoDeputados();
			//Camara.AtualizaInfoDeputadosCompleto();

			////Camara.ImportarMandatos();
			//Camara.DownloadFotosDeputados(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

			//Importação na nova estrutura
			//for (int ano = 2009; ano <= 2018; ano++)
			//{
			//	Camara.ImportarDespesas(tempPath, ano, true);
			//}
			//Camara.ImportarDespesas(tempPath, 2017, false);

			Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2017));
			Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2018));
			////Camara.AtualizaDeputadoValores();
			//Camara.ImportaPresencasDeputados();

			////Camara.ValidarLinkRecibos();
			////#endregion Camara

			////#region Senado
			////Senado.CarregaSenadores();
			////Senado.DownloadFotosSenadores(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\SENADOR\");

			//for (int ano = 2008; ano <= 2017; ano++)
			//{
			//	Senado.ImportarDespesas(tempPath, ano, true);
			//}
			//Senado.ImportarDespesas(tempPath, 2017, false);
			//#endregion Senado

			//Fornecedor.ConsultarReceitaWS();

			////Fornecedor.AtualizaFornecedorDoador();
			//Fornecedor.ConsultarCNPJ();
		}
	}
}