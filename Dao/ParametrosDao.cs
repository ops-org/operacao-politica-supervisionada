using MySql.Data.MySqlClient;
using OPS.Core;
using System;

namespace OPS.Dao
{
	public class ParametrosDao
	{
		public void CarregarPadroes()
		{
			try
			{
				using (Banco banco = new Banco())
				{
					using (MySqlDataReader reader = banco.ExecuteReader("SELECT * from parametros"))
					{
						if (reader.Read())
						{
							Padrao.DeputadoFederalMenorAno = int.Parse(reader["cf_deputado_menor_ano"].ToString());
							Padrao.DeputadoFederalUltimaAtualizacao = DateTime.Parse(reader["cf_deputado_ultima_atualizacao"].ToString());
							Padrao.SenadorMenorAno = int.Parse(reader["sf_senador_menor_ano"].ToString());
							Padrao.SenadorUltimaAtualizacao = DateTime.Parse(reader["sf_senador_ultima_atualizacao"].ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				var e = ex;
				// TODO: Logar Erro
			}
		}
	}
}