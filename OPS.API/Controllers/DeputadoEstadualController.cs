using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.Core;
using OPS.Core.DTO;
using OPS.Core.Repository;
using OPS.Infraestrutura;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoEstadualController : Controller
    {
        private readonly DeputadoEstadualRepository _deputadoEstadualRepository;

        public DeputadoEstadualController(DeputadoEstadualRepository deputadoEstadualRepository)
        {
            _deputadoEstadualRepository = deputadoEstadualRepository;
        }

        [HttpGet("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            return await _deputadoEstadualRepository.Consultar(id);
        }

        [HttpPost("Lista")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            return await _deputadoEstadualRepository.Lista(filtro);
        }

        [HttpPost("Pesquisa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            return await _deputadoEstadualRepository.Pesquisa(filtro);
        }

        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await _deputadoEstadualRepository.Lancamentos(request);
        }

        [HttpGet("TipoDespesa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            return await _deputadoEstadualRepository.TipoDespesa();
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            return await _deputadoEstadualRepository.MaioresNotas((uint)id);
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            return await _deputadoEstadualRepository.MaioresFornecedores((uint)id);
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            return await _deputadoEstadualRepository.GastosPorAno((uint)id);
        }

        [HttpGet]
        [Route("ResumoMensal")]
        public async Task<dynamic> ResumoMensal()
        {
            return await _deputadoEstadualRepository.ResumoMensal();
        }

        [HttpGet]
        [Route("ResumoAnual")]
        public async Task<dynamic> ResumoAnual()
        {
            return await _deputadoEstadualRepository.ResumoAnual();
        }
    }
}
