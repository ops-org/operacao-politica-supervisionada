using System.Collections.Generic;
using System.Web.Http;
using OPS.Core.DAO;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
    [RoutePrefix("Api/Indicadores")]
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class IndicadoresController : ApiController
    {
        [HttpGet]
        [Route("ParlamentarResumoGastos")]
        public dynamic ParlamentarResumoGastos()
        {
            return IndicadoresDao.ParlamentarResumoGastos();
        }

        [HttpGet]
        [Route("Busca")]
        public dynamic Busca([FromUri] string value)
        {
            var oDeputadoDao = new DeputadoDao();
            var oSenadorDao = new SenadorDao();
            var oFornecedorDao = new FornecedorDao();

            return new
            {
                deputado_federal = oDeputadoDao.Busca(value),
                senador = oSenadorDao.Busca(value),
                fornecedor = oFornecedorDao.Busca(value)
            };
        }
    }
}
