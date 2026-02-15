using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTOs;
using OPS.Core.Utilities;
using OPS.Infraestrutura;

namespace OPS.Core.Repositories
{
    public class InicioRepository : BaseRepository
    {
        public InicioRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ImportacaoInfoDTO>> InfoImportacao()
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
                .Select(x => new ImportacaoInfoDTO
                {
                    Id = x.i.Id,
                    Sigla = x.e != null ? x.e.Sigla : null,
                    Nome = x.e.Nome != null ? x.e.Nome : x.i.Chave,
                    Url = x.i.Url,
                    Info = x.i.Info,
                    UltimaDespesa = Utils.FormataData(x.i.UltimaDespesa),
                    UltimaImportacao = Utils.FormataData(x.i.DespesasFim)
                }).ToListAsync();

            return results;
        }

        /// <summary>
        /// Retorna resumo (8 Itens) dos parlamentares mais e menos gastadores
        /// 4 Deputados Federais MAIS gastadores (CEAP)
        /// 4 Deputados Estaduais MAIS gastadores (CEAP)
        /// 4 Senadores MAIS gastadores (CEAPS)
        /// </summary>
        /// <returns></returns>
        public async Task<ParlamentarResumoGastosDTO> ParlamentarResumoGastos()
        {
            var deputadosFederais = await _context.DeputadoCampeaoGastosCamara
                .OrderByDescending(d => d.ValorTotal)
                .Take(4)
                .Select(d => new ParlamentarResumoDTO
                {
                    Id = d.IdDeputado,
                    NomeParlamentar = d.NomeParlamentar,
                    ValorTotal = "R$ " + Utils.FormataValor(d.ValorTotal, 2),
                    SiglaPartidoEstado = d.SiglaPartido + " / " + d.SiglaEstado
                })
                .ToListAsync();

            var deputadosEstaduais = await _context.DeputadoCampeaoGastos
                .OrderByDescending(d => d.ValorTotal)
                .Take(4)
                .Select(d => new ParlamentarResumoDTO
                {
                    Id = d.IdDeputado,
                    NomeParlamentar = d.NomeParlamentar,
                    ValorTotal = "R$ " + Utils.FormataValor(d.ValorTotal, 2),
                    SiglaPartidoEstado = d.SiglaPartido + " / " + d.SiglaEstado
                })
                .ToListAsync();

            var senadores = await _context.SenadoresCampeaoGasto
                .OrderByDescending(s => s.ValorTotal)
                .Take(4)
                .Select(s => new ParlamentarResumoDTO
                {
                    Id = s.IdSenador,
                    NomeParlamentar = s.NomeParlamentar,
                    ValorTotal = "R$ " + Utils.FormataValor(s.ValorTotal, 2),
                    SiglaPartidoEstado = s.SiglaPartido + " / " + s.SiglaEstado
                })
                .ToListAsync();

            return new ParlamentarResumoGastosDTO()
            {
                Senadores = senadores,
                DeputadosFederais = deputadosFederais,
                DeputadosEstaduais = deputadosEstaduais
            };
        }
    }
}