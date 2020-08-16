using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Mvc;
using OPS.Core.DAO;
using System.Threading.Tasks;

namespace OPS.WebApi
{
    [ApiController]
    [Route("[controller]")]
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class PartidoController : Controller
    {
        PartidoDao dao;

        public PartidoController()
        {
            dao = new PartidoDao();
        }

        [HttpGet]
		[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
		public async Task<IActionResult> Consultar()
        {
            var result = await dao.Consultar();

			return Ok(result);
        }
    }
}
