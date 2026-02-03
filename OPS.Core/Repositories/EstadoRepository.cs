using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTOs;
using OPS.Infraestrutura;

namespace OPS.Core.Repositories
{
    public class EstadoRepository : BaseRepository
    {
        public EstadoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DropDownDTO>> Consultar()
        {
            IEnumerable<DropDownDTO> lista = await _context.Estados
                .OrderBy(x => x.Nome)
                .Select(x => new DropDownDTO()
                {
                    Id = x.Id,
                    Text = x.Nome + " (" + x.Sigla + ")",
                    HelpText = "Região " + x.Regiao,
                    Image = $"/img/estados/{x.Sigla.ToLower(System.Globalization.CultureInfo.CurrentCulture)}.png"
                }).ToListAsync();

            return lista;
        }
    }
}