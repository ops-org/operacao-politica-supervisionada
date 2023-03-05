using Microsoft.AspNetCore.Mvc;
using OPS.Core.DAO;
using System.Threading.Tasks;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class InicioController : Controller
    {
        [HttpGet]
        [Route("ParlamentarResumoGastos")]
        public dynamic ParlamentarResumoGastos()
        {
            return InicioRepository.ParlamentarResumoGastos();
        }

        [ResponseCache(VaryByQueryKeys = new[] { "q" })]
        [HttpGet]
        [Route("Busca")]
        public async Task<dynamic> Busca([FromQuery] string value)
        {
            var oDeputadoDao = new DeputadoRepository();
            var oSenadorDao = new SenadorRepository();
            var oFornecedorDao = new FornecedorRepository();

            return new
            {
                deputado_federal = await oDeputadoDao.Busca(value),
                senador = await oSenadorDao.Busca(value),
                fornecedor = await oFornecedorDao.Busca(value)
            };
        }
    }
}
