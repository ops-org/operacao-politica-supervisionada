using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.Core;
using OPS.Core.DTO;
using OPS.Core.Repository;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoEstadualController : Controller
    {
        DeputadoEstadualRepository dao;

        public DeputadoEstadualController()
        {
            dao = new DeputadoEstadualRepository();
        }

        [HttpGet("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            return await dao.Consultar(id);
        }

        [HttpPost("Lista")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            return await dao.Lista(filtro);
        }

        [HttpPost("Pesquisa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            return await dao.Pesquisa(filtro);
        }

        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await dao.Lancamentos(request);
        }

        [HttpGet("TipoDespesa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            return await dao.TipoDespesa();
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            return await dao.MaioresNotas(id);
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            return await dao.MaioresFornecedores(id);
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            return await dao.GastosPorAno(id);
        }

        [HttpGet]
        [Route("ResumoMensal")]
        public async Task<dynamic> ResumoMensal()
        {
            return await dao.ResumoMensal();
        }

        [HttpGet]
        [Route("ResumoAnual")]
        public async Task<dynamic> ResumoAnual()
        {
            return await dao.ResumoAnual();
        }
    }
}
