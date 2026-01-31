using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTOs;
using OPS.Infraestrutura;

namespace OPS.Core.Repositories
{
    public class PartidoRepository : BaseRepository
    {
        public PartidoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DropDownDTO>> Consultar()
        {
            var cuttofDate = DateTime.SpecifyKind(new System.DateTime(2008, 1, 1), DateTimeKind.Utc);

            IEnumerable<DropDownDTO> lista = await _context.Partidos
                .Where(x => x.Ativo || x.Extincao > cuttofDate)
                .OrderBy(x => x.Nome)
                .Select(x => new DropDownDTO()
                {
                    id = x.Id,
                    text = x.Nome,
                    helpText = $"{x.Sigla} - {x.Legenda}",
                    image = $"/img/partidos/{x.Imagem}"
                }).ToListAsync();

            return lista;
        }
    }
}