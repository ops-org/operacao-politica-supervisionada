using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.Core.DTO;
using OPS.Core.Repository;

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

        [HttpGet]
        [Route("Busca")]
        public async Task<dynamic> Busca([FromQuery] string value)
        {
            var oDeputadoDao = new DeputadoRepository();
            var oDeputadoEstadualDao = new DeputadoEstadualRepository();
            var oSenadorDao = new SenadorRepository();
            var oFornecedorDao = new FornecedorRepository();
            var filtro = new FiltroParlamentarDTO() { NomeParlamentar = value };

            return new
            {
                deputado_federal = await oDeputadoDao.Lista(filtro),
                deputado_estadual = await oDeputadoEstadualDao.Lista(filtro),
                senador = await oSenadorDao.Lista(filtro),
                fornecedor = await oFornecedorDao.Busca(value)
            };
        }

        [HttpGet]
        [Route("Importacao")]
        public async Task<dynamic> Importacao()
        {
            return InicioRepository.InfoImportacao();
        }
    }
}
