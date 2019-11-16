using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Mvc;
using OPS.Core.DAO;
using System.Threading.Tasks;

namespace OPS.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class IndicadoresController : Controller
    {
        [HttpGet]
        [Route("ParlamentarResumoGastos")]
        public dynamic ParlamentarResumoGastos()
        {
            return IndicadoresDao.ParlamentarResumoGastos();
        }

        [HttpGet]
        [Route("Busca")]
        public async Task<dynamic> Busca([FromQuery]string value)
        {
            var oDeputadoDao = new DeputadoDao();
            var oSenadorDao = new SenadorDao();
            var oFornecedorDao = new FornecedorDao();

            return new
            {
                deputado_federal = await oDeputadoDao.Busca(value),
                senador = oSenadorDao.Busca(value),
                fornecedor = oFornecedorDao.Busca(value)
            };
        }
    }
}
