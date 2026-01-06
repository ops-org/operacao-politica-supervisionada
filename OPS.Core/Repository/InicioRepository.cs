using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPS.Core.Utilities;
using OPS.Infraestrutura;

namespace OPS.Core.Repository
{
    public class InicioRepository : BaseRepository
    {
        public InicioRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<dynamic>> InfoImportacao()
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = @"
SELECT
	i.id, e.sigla, coalesce(e.nome, i.chave) as nome, i.url, i.info, i.ultima_despesa, i.despesas_fim
FROM importacao i
LEFT JOIN estado e ON e.id = i.id
ORDER BY e.nome, i.id";

                var results = new List<dynamic>();
                using (var reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new
                        {
                            id = reader["id"],
                            sigla = reader["sigla"].ToString(),
                            nome = reader["nome"].ToString(),
                            url = reader["url"].ToString(),
                            info = reader["info"].ToString(),
                            ultima_despesa = Utils.FormataData(reader["ultima_despesa"]),
                            ultima_importacao = Utils.FormataData(reader["despesas_fim"])
                        });
                    }
                }
                return results;
            }
        }

        /// <summary>
        /// Retorna resumo (8 Itens) dos parlamentares mais e menos gastadores
        /// 4 Deputados Federais MAIS gastadores (CEAP)
        /// 4 Deputados Estaduais MAIS gastadores (CEAP)
        /// 4 Senadores MAIS gastadores (CEAPS)
        /// </summary>
        /// <returns></returns>
        public object ParlamentarResumoGastos()
        {
            var deputadosFederais = _context.DeputadoCampeaoGastosCamara
                .OrderByDescending(d => d.ValorTotal)
                .Take(4)
                .Select(d => new
                {
                    id_cf_deputado = d.IdDeputado,
                    nome_parlamentar = d.NomeParlamentar,
                    valor_total = "R$ " + Utils.FormataValor(d.ValorTotal),
                    sigla_partido_estado = d.SiglaPartido + " / " + d.SiglaEstado
                })
                .ToList();

            var deputadosEstaduais = _context.DeputadoCampeaoGastos
                .OrderByDescending(d => d.ValorTotal)
                .Take(4)
                .Select(d => new
                {
                    id_cl_deputado = d.IdDeputado,
                    nome_parlamentar = d.NomeParlamentar,
                    valor_total = "R$ " + Utils.FormataValor(d.ValorTotal),
                    sigla_partido_estado = d.SiglaPartido + " / " + d.SiglaEstado
                })
                .ToList();

            var senadores = _context.SenadoresCampeaoGasto
                .OrderByDescending(s => s.ValorTotal)
                .Take(4)
                .Select(s => new
                {
                    id_sf_senador = s.IdSenador,
                    nome_parlamentar = s.NomeParlamentar,
                    valor_total = "R$ " + Utils.FormataValor(s.ValorTotal),
                    sigla_partido_estado = s.SiglaPartido + " / " + s.SiglaEstado
                })
                .ToList();

            return new
            {
                senado = senadores,
                camara_federal = deputadosFederais,
                camara_estadual = deputadosEstaduais
            };
        }
    }
}