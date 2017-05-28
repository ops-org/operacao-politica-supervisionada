using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OPS.Core;
using OPS.ImportacaoDados;

namespace OPS.ImportacaoManual
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Padrao.ConnectionString = ConfigurationManager.ConnectionStrings["AuditoriaContext"].ToString();

			var tempPath = @"C:\GitHub\operacao-politica-supervisionada\OPS\temp";

			#region Camara
			//Camara.AtualizaInfoDeputados();
			//Camara.AtualizaInfoDeputadosCompleto();

			//Camara.ImportarMandatos();
			//Camara.DownloadFotosDeputados(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

			//Importação na nova estrutura
			//for (int ano = 2009; ano <= 2017; ano++)
			//{
			//	Senado.ImportarDespesas(tempPath, ano, true);
			//}
			//Camara.ImportarDespesas(tempPath, 2017, false);

			Camara.ImportaPresencasDeputados();
			#endregion Camara

			#region Senado
			//Senado.CarregaSenadores();
			//Senado.DownloadFotosSenadores(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\SENADOR\");
			
			//for (int ano = 2008; ano <= 2017; ano++)
			//{
			//   Senado.ImportarDespesas(tempPath, ano, true);
			//}
			//Senado.ImportarDespesas(tempPath, 2017, false);

			//CamaraAtualizaDeputadoValores();
			//Senado.AtualizaSenadorValores();
			#endregion Senado

			//Fornecedor.ConsultarReceitaWS();

			//Fornecedor.AtualizaFornecedorDoador();

			// To search and replace content in a document part.
			//SearchAndReplace(@"C:\GitHub\operacao-politica-supervisionada\OPS\temp\Modelo Padrão de Denúncia por falta de Transparência versão 4.docx");
		}


	}
}