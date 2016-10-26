using MySql.Data.MySqlClient;
using OPS.Core;
using System.Collections.Generic;
using System.Text;

namespace OPS.Dao
{
	public class EstadoDao
	{
		internal dynamic Consultar()
		{
			using (Banco banco = new Banco())
			{
				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader("SELECT id, sigla, nome FROM estado order by nome;"))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader["id"].ToString(),
							tokens = reader["sigla"].ToString(),
							text = reader["nome"].ToString()
						});
					}
				}
				return lstRetorno;
			}
		}
	}
}