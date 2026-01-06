using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTO;
using OPS.Infraestrutura;

namespace OPS.Core.Repository
{
    public class PartidoRepository : BaseRepository
    {
        public PartidoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DropDownDTO>> Consultar()
        {
            IEnumerable<DropDownDTO> lista = await _context.Partidos
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