using OPS.Core;
using System.Data;
using System;
using System.Text;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

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
		/// 4 Deputados MAIS gastadores (CEAP)
		/// 4 Senadores MAIS gastadores (CEAPS)
		/// </summary>
		/// <returns></returns>
		internal static object RecuperarCardsIndicadores()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();

				strSql.Append("SELECT idParlamentar as Id, nomeParlamentar as NomeParlamentar, vlrTotal as ValorTotal, sgPartido as Partido, sgUf as Uf ");
				strSql.Append("FROM deputado_campeao_gasto ");
				strSql.Append("order by 3 desc; ");

				strSql.Append("SELECT idParlamentar as Id, nomeParlamentar as NomeParlamentar, vlrTotal as ValorTotal, sgPartido as Partido, sgUf as Uf ");
				strSql.Append("FROM senador_campeao_gasto ");
				strSql.Append("order by 3 desc; ");

				var lstDeputados = new List<dynamic>();
				var lstSenadores = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstDeputados.Add(new
						{
							Id = reader[0].ToString(),
							NomeParlamentar = reader[1].ToString(),
							ValorTotal = "R$ " + Utils.FormataValor(reader[2]),
							PartidoEstado = string.Format("{0} / {1}", reader[3], reader[4])
						});
					}

					reader.NextResult();
					while (reader.Read())
					{
						lstSenadores.Add(new
						{
							Id = reader[0].ToString(),
							NomeParlamentar = reader[1].ToString(),
							ValorTotal = "R$ " + Utils.FormataValor(reader[2]),
							PartidoEstado = string.Format("{0} / {1}", reader[3], reader[4])
						});
					}

					return new
					{
						CamaraFederal = lstDeputados,
						Senado = lstSenadores
					};
				}
			}
		}
	}
}