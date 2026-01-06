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
        private readonly DeputadoRepository _deputadoRepository;
        private readonly DeputadoEstadualRepository _deputadoEstadualRepository;
        private readonly SenadorRepository _senadorRepository;
        private readonly FornecedorRepository _fornecedorRepository;
        private readonly InicioRepository _inicioRepository;

        public InicioController(
            DeputadoRepository deputadoRepository,
            DeputadoEstadualRepository deputadoEstadualRepository,
            SenadorRepository senadorRepository,
            FornecedorRepository fornecedorRepository,
            InicioRepository inicioRepository)
        {
            _deputadoRepository = deputadoRepository;
            _deputadoEstadualRepository = deputadoEstadualRepository;
            _senadorRepository = senadorRepository;
            _fornecedorRepository = fornecedorRepository;
            _inicioRepository = inicioRepository;
        }

        [HttpGet]
        [Route("ParlamentarResumoGastos")]
        public dynamic ParlamentarResumoGastos()
        {
            return _inicioRepository.ParlamentarResumoGastos();
        }

        [HttpGet]
        [Route("Busca")]
        public async Task<dynamic> Busca([FromQuery] string value)
        {
            var filtro = new FiltroParlamentarDTO() { NomeParlamentar = value };

            return new
            {
                deputado_federal = await _deputadoRepository.Lista(filtro),
                deputado_estadual = await _deputadoEstadualRepository.Lista(filtro),
                senador = await _senadorRepository.Lista(filtro),
                fornecedor = await _fornecedorRepository.Busca(value)
            };
        }

        [HttpGet]
        [Route("Importacao")]
        public async Task<dynamic> Importacao()
        {
            return await _inicioRepository.InfoImportacao();
        }
    }
}
