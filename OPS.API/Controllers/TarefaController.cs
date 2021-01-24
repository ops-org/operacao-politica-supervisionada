using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core.DAO;

namespace OPS.WebApi
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : Controller
    {

        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public IApiCacheOutput Cache { get; }

        public TarefaController(IConfiguration configuration, IWebHostEnvironment env, IApiCacheOutput cache)
        {
            Environment = env;
            Configuration = configuration;
            Cache = cache;
        }

        [HttpGet]
        [Route("LimparCache")]
        public async void LimparCache()
        {
            new ParametrosDao().CarregarPadroes();

            await Cache.RemoveStartsWithAsync("*");
        }

        
    }
}