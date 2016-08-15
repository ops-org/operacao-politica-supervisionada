using OPS.Core;
using System.Data;
using System;

namespace OPS.Dao
{
	public static class ComandoSqlDao
	{
		static object everoneUseThisLockObject4CacheComandoSQL = new object();

		public enum eGrupoComandoSQL
		{
			/// <summary>
			/// Resumo da Auditoria da Pagina Principal
			/// </summary>
			ResumoAuditoria = 1
		}

		/// <summary>
		/// Executa Consultas Armazenadas que retornam resultado unico (Ex: Select Count(x) ...)
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="Grupo"></param>
		/// <returns></returns>
		internal static DataTable ExecutarConsultaSimples(eGrupoComandoSQL Grupo)
		{
			string grupo = Grupo.GetHashCode().ToString();
			DataTable dtResultadoComandoSQL = MemoryCacheHelper.GetCachedData<DataTable>("ResultadoComandoSQL" + grupo);
			if (dtResultadoComandoSQL == null)
			{
				using (Banco banco = new Banco())
				{
					dtResultadoComandoSQL = banco.GetTable("SELECT Nome, ComandoSQL, '' as Resultado FROM comandosql WHERE Grupo=" + grupo + " ORDER BY Ordem");

					if (dtResultadoComandoSQL != null)
					{
						foreach (DataRow row in dtResultadoComandoSQL.Rows)
						{
							row["Resultado"] = banco.ExecuteScalar(row["ComandoSQL"].ToString()).ToString();
						}
					}
				}

				MemoryCacheHelper.AddCacheData("ResultadoComandoSQL" + grupo, dtResultadoComandoSQL, everoneUseThisLockObject4CacheComandoSQL, 60 * 24);
			}

			return dtResultadoComandoSQL;
		}

		/// <summary>
		/// Retorna resumo (8 Itens) dos parlamentares mais e menos gastadores
		/// 2 Deputados MAIS gastadores (CEAP)
		/// 2 Senadores MAIS gastadores (CEAPS)
		/// 2 Deputados MENOS gastadores (CEAP)
		/// 2 Senadores MENOS gastadores (CEAPS)
		/// </summary>
		/// <returns></returns>
		internal static object RecuperarCardsIndicadores()
		{
			using (Banco banco = new Banco())
			{
				//TODO: Formatar R$ {0:#,##0.00} - valorCard
				return banco.GetTable("SELECT * FROM cards_bi order by ordemExibicao");
			}
		}
	}
}