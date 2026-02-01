using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("deputado-estadual")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoEstadualController : Controller
    {
        private readonly DeputadoEstadualRepository _deputadoEstadualRepository;
        private readonly HybridCache _hybridCache;

        public DeputadoEstadualController(DeputadoEstadualRepository deputadoEstadualRepository, HybridCache hybridCache)
        {
            _deputadoEstadualRepository = deputadoEstadualRepository;
            _hybridCache = hybridCache;
        }

        [HttpGet("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            var cacheKey = $"deputado-estadual-consultar-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.Consultar(id);
            });
        }

        [HttpPost("Lista")]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            var cacheKey = $"deputado-estadual-lista-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.Lista(filtro);
            });
        }

        [HttpPost("Pesquisa")]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            var cacheKey = $"deputado-estadual-pesquisa-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.Pesquisa(filtro);
            });
        }

        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            var cacheKey = $"deputado-estadual-lancamentos-{JsonSerializer.Serialize(request)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.Lancamentos(request);
            });
        }

        [HttpGet("TipoDespesa")]
        public async Task<dynamic> TipoDespesa()
        {
            const string cacheKey = "deputado-estadual-tipo-despesa";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.TipoDespesa();
            });
        }

        [HttpGet("Documento/{id:int}")]
        public async Task<ActionResult<DocumentoDetalheDTO>> Documento(int id)
        {
            var cacheKey = $"deputado-estadual-documento-{id}";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.Documento(id);
            });

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            var cacheKey = $"deputado-estadual-documentos-mesmo-dia-{id}";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.DocumentosDoMesmoDia(id);
            });

            return Ok(result);
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            var cacheKey = $"deputado-estadual-documentos-subcota-mes-{id}";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.DocumentosDaSubcotaMes(id);
            });

            return Ok(result);
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            var cacheKey = $"deputado-estadual-maiores-notas-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.MaioresNotas((int)id);
            });
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            var cacheKey = $"deputado-estadual-maiores-fornecedores-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.MaioresFornecedores((int)id);
            });
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            var cacheKey = $"deputado-estadual-gastos-por-ano-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.GastosPorAno((int)id);
            });
        }

        [HttpGet]
        [Route("ResumoMensal")]
        public async Task<dynamic> ResumoMensal()
        {
            const string cacheKey = "deputado-estadual-resumo-mensal";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.ResumoMensal();
            });
        }

        [HttpGet]
        [Route("ResumoAnual")]
        public async Task<dynamic> ResumoAnual()
        {
            const string cacheKey = "deputado-estadual-resumo-anual";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.ResumoAnual();
            });
        }

        [HttpGet]
        [Route("{id:int}/CustoAnual")]
        public async Task<dynamic> CustoAnual(int id)
        {
            var cacheKey = $"deputado-estadual-custo-anual-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _deputadoEstadualRepository.CustoAnual(id);
            });
        }
    }
}
