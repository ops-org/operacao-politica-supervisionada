using MySql.Data.MySqlClient;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OPS.Dao
{
	public class ParametrosDao
	{
		internal void CarregarPadroes()
		{
			try
			{
				using (Banco banco = new Banco())
				{
					using (MySqlDataReader reader = banco.ExecuteReader("SELECT * from parametros"))
					{
						if (reader.Read())
						{
							Padrao.DeputadoFederalMenorAno = int.Parse(reader["menorAno"].ToString());
							Padrao.DeputadoFederalUltimaAtualizacao = DateTime.Parse(reader["ultima_atualizacao"].ToString());
							Padrao.SenadorMenorAno = int.Parse(reader["menorAnoSenadores"].ToString());
							Padrao.SenadorUltimaAtualizacao = DateTime.Parse(reader["ultimaAtualizacaoSenadores"].ToString());
						}
					}
				}
			}
			catch (Exception)
			{
				// TODO: Logar Erro
			}
		}
	}
}