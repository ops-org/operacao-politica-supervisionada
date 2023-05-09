using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core.DAO;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : Controller
    {

        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public TarefaController(IConfiguration configuration, IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        [HttpGet]
        [Route("LimparCache")]
        public void LimparCache()
        {
            new ParametrosRepository().CarregarPadroes();
        }


    }
}