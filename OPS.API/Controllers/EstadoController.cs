using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class EstadoController : Controller
    {
        EstadoRepository repository;
        private readonly HybridCache _hybridCache;

        public EstadoController(EstadoRepository repository, HybridCache hybridCache)
        {
            this.repository = repository;
            _hybridCache = hybridCache;
        }

        [HttpGet]
        public async Task<IActionResult> Consultar()
        {
            const string cacheKey = "estado-consultar";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await repository.Consultar();
            });

            return Ok(result);
        }
    }
}
