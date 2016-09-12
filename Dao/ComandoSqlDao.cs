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
			var result = CacheHelper.Get<dynamic>("CampeoesDeGastos");

			if (result == null)
			{
				using (Banco banco = new Banco())
				{
					var strSql = new StringBuilder();
					strSql.Append("SELECT l.ideCadastro as Id, p.txNomeParlamentar as NomeParlamentar, sum(l.vlrLiquido) as ValorTotal, p.Partido, p.Uf ");
					strSql.Append("FROM lancamentos l ");
					strSql.Append("INNER JOIN parlamentares p on p.ideCadastro = l.ideCadastro ");
					strSql.Append("where l.AnoMes >= 201502 ");
					strSql.Append("group by 1, 2 ");
					strSql.Append("order by 3 desc ");
					strSql.AppendLine("limit 4; ");

					strSql.Append("SELECT l.CodigoParlamentar as Id, s.NomeParlamentar as NomeParlamentar, sum(l.Valor) as ValorTotal, s.SiglaPartido as Partido, s.siglaUf as Uf ");
					strSql.Append("FROM lancamentos_senadores l ");
					strSql.Append("INNER JOIN senadores s on s.CodigoParlamentar = l.CodigoParlamentar ");
					strSql.Append("where l.AnoMes >= 201002 ");
					strSql.Append("group by 1, 2 ");
					strSql.Append("order by 3 desc ");
					strSql.AppendLine("limit 4; ");

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

						result = new
						{
							CamaraFederal = lstDeputados,
							Senado = lstSenadores
						};
					}

					CacheHelper.Add<dynamic>("CampeoesDeGastos", result);
				}
			}

			return result;
		}
	}
}