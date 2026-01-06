using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTO;
using OPS.Infraestrutura;

namespace OPS.Core.Repository
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
                    id = x.Id,
                    text = x.Nome + " (" + x.Sigla + ")",
                    helpText = "Região " + x.Regiao,
                    image = $"/img/estados/{x.Sigla.ToLower(System.Globalization.CultureInfo.CurrentCulture)}.png"
                }).ToListAsync();

            return lista;
        }
    }
}