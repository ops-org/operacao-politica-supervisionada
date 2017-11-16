using System;
using MySql.Data.MySqlClient;

namespace OPS.Core.DAO
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
							Padrao.DeputadoFederalUltimaAtualizacao = DateTime.Parse(reader["cf_deputado_ultima_atualizacao"].ToString());
							Padrao.SenadorUltimaAtualizacao = DateTime.Parse(reader["sf_senador_ultima_atualizacao"].ToString());
							Padrao.DeputadoFederalPresencaUltimaAtualizacao = DateTime.Parse(reader["cf_deputado_presenca_ultima_atualizacao"].ToString());
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