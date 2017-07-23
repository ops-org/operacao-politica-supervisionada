using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace OPS.Core.DAO
{
    public class EstadoDao
    {
	    public dynamic Consultar()
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