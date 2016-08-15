using MySql.Data.MySqlClient;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OPS.Dao
{
	public class UsuarioDao
	{
		internal dynamic Pesquisa(FiltroUsuarioDTO filtro)
		{
			StringBuilder strSql = new StringBuilder();

			strSql.Append("SELECT SQL_CALC_FOUND_ROWS u.UserName as Nome, u.Email ");
			strSql.Append("FROM users u ");
			strSql.Append("INNER JOIN users_detail ud ON u.Username = ud.Username ");
			strSql.Append("WHERE ud.uf = @uf ");
			strSql.Append("AND ud.mostra_email = 1 ");

			strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "u.UserName ASC" : filtro.sorting);
			strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

			strSql.Append("SELECT FOUND_ROWS(); ");

			using (Banco banco = new Banco())
			{
				banco.AddParameter("uf", filtro.uf);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					int NomeOrdinal = reader.GetOrdinal("Nome");
					int EmailOrdinal = reader.GetOrdinal("Email");

					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							Nome = reader[NomeOrdinal],
							Email = reader[EmailOrdinal]
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
	}
}