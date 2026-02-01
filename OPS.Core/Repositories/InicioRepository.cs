using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.Utilities;
using OPS.Infraestrutura;

namespace OPS.Core.Repositories
{
    public class InicioRepository : BaseRepository
    {
        public InicioRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<dynamic>> InfoImportacao()
        {
            var results = await _context.Importacoes
                .GroupJoin(_context.Estados,
                    i => i.IdEstado,
                    e => e.Id,
                    (i, estados) => new { i, estados })
                .SelectMany(
                    x => x.estados.DefaultIfEmpty(),
                    (x, e) => new { x.i, e })
                .OrderBy(x => x.e.Nome)
                .ThenBy(x => x.i.Id)
                .Select(x => new
                {
                    id = x.i.Id,
                    sigla = x.e.Sigla,
                    nome = x.e.Nome != null ? x.e.Nome : x.i.Chave,
                    url = x.i.Url,
                    info = x.i.Info,
                    ultima_despesa = Utils.FormataData(x.i.UltimaDespesa),
                    ultima_importacao = Utils.FormataData(x.i.DespesasFim)
                })
                .ToListAsync();

            return results;
        }

        /// <summary>
        /// Retorna resumo (8 Itens) dos parlamentares mais e menos gastadores
        /// 4 Deputados Federais MAIS gastadores (CEAP)
        /// 4 Deputados Estaduais MAIS gastadores (CEAP)
        /// 4 Senadores MAIS gastadores (CEAPS)
        /// </summary>
        /// <returns></returns>
        public async Task<object> ParlamentarResumoGastos()
        {
            var deputadosFederais = await _context.DeputadoCampeaoGastosCamara
                .OrderByDescending(d => d.ValorTotal)
                .Take(4)
                .Select(d => new
                {
                    id_cf_deputado = d.IdDeputado,
                    nome_parlamentar = d.NomeParlamentar,
                    valor_total = "R$ " + Utils.FormataValor(d.ValorTotal),
                    sigla_partido_estado = d.SiglaPartido + " / " + d.SiglaEstado
                })
                .ToListAsync();

            var deputadosEstaduais = await _context.DeputadoCampeaoGastos
                .OrderByDescending(d => d.ValorTotal)
                .Take(4)
                .Select(d => new
                {
                    id_cl_deputado = d.IdDeputado,
                    nome_parlamentar = d.NomeParlamentar,
                    valor_total = "R$ " + Utils.FormataValor(d.ValorTotal),
                    sigla_partido_estado = d.SiglaPartido + " / " + d.SiglaEstado
                })
                .ToListAsync();

            var senadores = await _context.SenadoresCampeaoGasto
                .OrderByDescending(s => s.ValorTotal)
                .Take(4)
                .Select(s => new
                {
                    id_sf_senador = s.IdSenador,
                    nome_parlamentar = s.NomeParlamentar,
                    valor_total = "R$ " + Utils.FormataValor(s.ValorTotal),
                    sigla_partido_estado = s.SiglaPartido + " / " + s.SiglaEstado
                })
                .ToListAsync();

            return new
            {
                senado = senadores,
                camara_federal = deputadosFederais,
                camara_estadual = deputadosEstaduais
            };
        }
    }
}