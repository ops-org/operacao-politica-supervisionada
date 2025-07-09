using System;
using Serilog;

namespace OPS.Core.Repository
{
    public class ParametrosRepository
    {
        public void CarregarPadroes()
        {
            try
            {
                using (AppDb banco = new AppDb())
                {
                    using (var reader = banco.ExecuteReader("SELECT * from parametros"))
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
                Log.Error(ex, ex.Message);
            }
        }
    }
}