using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.Core.Repository;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class EstadoController : Controller
    {
        EstadoRepository repository;

        public EstadoController(EstadoRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<IActionResult> Consultar()
        {
            var result = await repository.Consultar();

            return Ok(result);
        }
    }
}
