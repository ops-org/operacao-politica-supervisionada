using System;
using System.Collections.Generic;
using System.Text;
using OPS.Core.Utilities;

namespace OPS.Core.Repository
{
    public static class InicioRepository
    {
        public static IEnumerable<dynamic> InfoImportacao()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = @"
SELECT
	i.id, e.sigla, coalesce(e.nome, i.chave) as nome, i.url, i.info, i.ultima_despesa, i.despesas_fim
FROM importacao i
LEFT JOIN estado e ON e.id = i.id
ORDER BY e.nome, i.id";

                using (var reader = banco.ExecuteReader(strSql.ToString()))
                {
                    while (reader.Read())
                    {
                        yield return new
                        {
                            id = reader["id"],
                            sigla = reader["sigla"].ToString(),
                            nome = reader["nome"].ToString(),
                            url = reader["url"].ToString(),
                            info = reader["info"].ToString(),
                            ultima_despesa = Utils.FormataData(reader["ultima_despesa"]),
                            ultima_importacao = Utils.FormataData(reader["despesas_fim"])
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Retorna resumo (8 Itens) dos parlamentares mais e menos gastadores
        /// 4 Deputados Federais MAIS gastadores (CEAP)
        /// 4 Deputados Estaduais MAIS gastadores (CEAP)
        /// 4 Senadores MAIS gastadores (CEAPS)
        /// </summary>
        /// <returns></returns>
        public static object ParlamentarResumoGastos()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.Append(@"
					SELECT id_cf_deputado, nome_parlamentar, valor_total, sigla_partido, sigla_estado
					FROM cf_deputado_campeao_gasto
					order by valor_total desc; "
                );

                strSql.Append(@"
					SELECT id_cl_deputado, nome_parlamentar, valor_total, sigla_partido, sigla_estado
					FROM cl_deputado_campeao_gasto
					order by valor_total desc; "
                );

                strSql.Append(@"
					SELECT id_sf_senador, nome_parlamentar, valor_total, sigla_partido, sigla_estado
					FROM sf_senador_campeao_gasto
					order by valor_total desc; "
                );

                var lstDeputadosFederais = new List<dynamic>();
                var lstDeputadosEstaduais = new List<dynamic>();
                var lstSenadores = new List<dynamic>();
                using (var reader = banco.ExecuteReader(strSql.ToString()))
                {
                    while (reader.Read())
                    {
                        lstDeputadosFederais.Add(new
                        {
                            id_cf_deputado = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            valor_total = "R$ " + Utils.FormataValor(reader["valor_total"]),
                            sigla_partido_estado = string.Format("{0} / {1}", reader["sigla_partido"], reader["sigla_estado"])
                        });
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        lstDeputadosEstaduais.Add(new
                        {
                            id_cl_deputado = reader["id_cl_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            valor_total = "R$ " + Utils.FormataValor(reader["valor_total"]),
                            sigla_partido_estado = string.Format("{0} / {1}", reader["sigla_partido"], reader["sigla_estado"])
                        });
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        lstSenadores.Add(new
                        {
                            id_sf_senador = reader["id_sf_senador"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            valor_total = "R$ " + Utils.FormataValor(reader["valor_total"]),
                            sigla_partido_estado = string.Format("{0} / {1}", reader["sigla_partido"], reader["sigla_estado"])
                        });
                    }

                    return new
                    {
                        senado = lstSenadores,
                        camara_federal = lstDeputadosFederais,
                        camara_estadual = lstDeputadosEstaduais
                    };
                }
            }
        }
    }
}