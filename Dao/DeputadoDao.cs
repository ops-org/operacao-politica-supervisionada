using MySql.Data.MySqlClient;
using OPS.Core;
using PetAzul.Models.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OPS.Dao
{
	public class DeputadoDao
	{
		public dynamic LancamentosPorParlamentar(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
				strSql.Append("cl.IdeCadastro, cl.NomeParlamentar, cl.SgUF, cl.SgPartido, SUM(cl.VlrLiquido) AS VlrTotal ");
				strSql.Append("FROM camara_lancamento cl ");
				strSql.Append("WHERE cl.IdeCadastro > 0 ");
				strSql.Append("GROUP BY cl.IdeCadastro, cl.NomeParlamentar, cl.SgUF, cl.SgPartido ");
				strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
				strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				strSql.Append("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							IdeCadastro = reader[0],
							NomeParlamentar = reader[1],
							SgUF = reader[2],
							SgPartido = reader[3],
							VlrTotal = Utils.FormataValor(reader[4])
						});
					}

					reader.NextResult();
					reader.Read();

					return new
					{
						TotalCount = reader[0],
						Results = lstRetorno
					};
				}
			}
		}

		internal dynamic Pesquisa(FiltroDropDownDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS ideCadastro, txNomeParlamentar FROM parlamentares ");
				if (!string.IsNullOrEmpty(filtro.q))
				{
					strSql.AppendFormat("WHERE txNomeParlamentar LIKE '%{0}%' ", filtro.q);
				}
				strSql.AppendFormat("ORDER BY txNomeParlamentar ");
				strSql.AppendFormat("LIMIT {0},{1}; ", ((filtro.page ?? 1) - 1) * filtro.count, filtro.count);

				strSql.Append("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader[0].ToString(),
							text = reader[1].ToString(),
						});
					}

					reader.NextResult();
					reader.Read();

					return new
					{
						total_count = reader[0],
						results = lstRetorno
					};
				}
			}
		}
	}
}