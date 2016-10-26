using System;

namespace OPS.Core
{
	public static class Padrao
	{
		public static string ConnectionString;

		/// <summary>
		/// Quantidade padrão de Itens a Exibir nas Grids
		/// </summary>
		public static readonly int ITENS_POR_PAGINA = 100;

		/// <summary>
		/// Versão do Site
		/// </summary>
		public static readonly int VERSAO = 160908;

		/// <summary>
		/// Ano Inicial dos Dados de Auditoria dos Senadores
		/// </summary>
		public static int DeputadoFederalMenorAno;

		/// <summary>
		/// Data da Ultima Importação da CEAP (Cota dos Deputados)
		/// </summary>
		public static DateTime DeputadoFederalUltimaAtualizacao;

		/// <summary>
		/// Ano Inicial dos Dados de Auditoria dos Senadores
		/// </summary>
		public static int SenadorMenorAno;

		/// <summary>
		/// Data da Ultima Importação da CEAPs (Cota dos Senadores)
		/// </summary>
		public static DateTime SenadorUltimaAtualizacao;
	}
}
