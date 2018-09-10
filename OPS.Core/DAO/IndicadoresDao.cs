using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace OPS.Core.DAO
{
	public static class IndicadoresDao
	{
		/// <summary>
		/// Retorna resumo (8 Itens) dos parlamentares mais e menos gastadores
		/// 4 Deputados MAIS gastadores (CEAP)
		/// 4 Senadores MAIS gastadores (CEAPS)
		/// </summary>
		/// <returns></returns>
		public static object ParlamentarResumoGastos()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();

				strSql.Append(@"
					SELECT id_cf_deputado, nome_parlamentar, valor_total, sigla_partido, sigla_estado
					FROM cf_deputado_campeao_gasto
					order by valor_total desc; "
				);

				strSql.Append(@"
					SELECT id_sf_senador, nome_parlamentar, valor_total, sigla_partido, sigla_estado
					FROM sf_senador_campeao_gasto
					order by valor_total desc; "
				);

				var lstDeputados = new List<dynamic>();
				var lstSenadores = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstDeputados.Add(new
						{
							id_cf_deputado = reader["id_cf_deputado"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
							valor_total = "R$ " + Utils.FormataValor(reader["valor_total"]),
							sigla_partido_estado = string.Format("{0} / {1}", reader["sigla_partido"], reader["sigla_estado"])
						});
					}

					reader.NextResult();
					while (reader.Read())
					{
						lstSenadores.Add(new
						{
							id_sf_senador = reader["id_sf_senador"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
							valor_total = "R$ " + Utils.FormataValor(reader["valor_total"]),
							sigla_partido_estado = string.Format("{0} / {1}", reader["sigla_partido"], reader["sigla_estado"])
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