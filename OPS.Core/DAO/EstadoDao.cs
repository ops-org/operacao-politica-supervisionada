using System.Collections.Generic;

namespace OPS.Core.DAO
{
    public class EstadoDao
    {
	    public dynamic Consultar()
        {
            using (AppDb banco = new AppDb())
            {
                var lstRetorno = new List<dynamic>();
                using (var reader = banco.ExecuteReader("SELECT id, sigla, nome FROM estado order by nome;"))
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