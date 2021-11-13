using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Mvc;
using OPS.Core.DAO;
using System.Threading.Tasks;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class EstadoController : Controller
    {
        EstadoDao dao;

        public EstadoController()
        {
            dao = new EstadoDao();
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
