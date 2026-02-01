using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartidoController : Controller
    {
        PartidoRepository repository;
        private readonly HybridCache _hybridCache;

        public PartidoController(PartidoRepository repository, HybridCache hybridCache)
        {
            this.repository = repository;
            _hybridCache = hybridCache;
        }

        [HttpGet]
        public async Task<IActionResult> Consultar()
        {
            const string cacheKey = "partido-consultar";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await repository.Consultar();
            });

            return Ok(result);
        }
    }
}
