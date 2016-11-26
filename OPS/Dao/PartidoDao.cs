using MySql.Data.MySqlClient;
using OPS.Core;
using System.Collections.Generic;

namespace OPS.Dao
{
	public class PartidoDao
	{
		internal dynamic Consultar()
		{
			using (Banco banco = new Banco())
			{
				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader("SELECT id, sigla, nome FROM partido order by nome;"))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader["id"].ToString(),
                            tokens = new[] { reader["sigla"].ToString(), reader["nome"].ToString() },
                            text = reader["nome"].ToString()
						});
					}
				}
				return lstRetorno;
			}
		}
	}
}