using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.API.Services;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    public abstract class ParlamentarBaseController<TRepository> : Controller 
        where TRepository : IParlamentarRepository
    {
        protected readonly TRepository _repository;
        protected readonly IHybridCacheService _hybridCacheService;
        protected abstract string CachePrefix { get; }

        protected ParlamentarBaseController(TRepository repository, IHybridCacheService hybridCacheService)
        {
            _repository = repository;
            _hybridCacheService = hybridCacheService;
        }

        [HttpGet("{id:int}")]
        public virtual async Task<ParlamentarDetalheDTO> Consultar(int id)
        {
            var cacheKey = $"{CachePrefix}-consultar-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Consultar(id);
            });
        }

        [HttpPost("Lista")]
        public virtual async Task<List<ParlamentarListaDTO>> Lista(FiltroParlamentarDTO filtro)
        {
            var cacheKey = $"{CachePrefix}-lista-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Lista(filtro);
            });
        }

        [HttpPost("Pesquisa")]
        public virtual async Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro)
        {
            var cacheKey = $"{CachePrefix}-pesquisa-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Pesquisa(filtro);
            });
        }

        [HttpPost("Lancamentos")]
        public virtual async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            var cacheKey = $"{CachePrefix}-lancamentos-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Lancamentos(request);
            });
        }

        [HttpGet("TipoDespesa")]
        public virtual async Task<List<TipoDespesaDTO>> TipoDespesa()
        {
            var cacheKey = $"{CachePrefix}-tipo-despesa";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.TipoDespesa();
            });
        }

        [HttpGet("Documento/{id:int}")]
        public virtual async Task<ActionResult<DocumentoDetalheDTO>> Documento(int id)
        {
            var cacheKey = $"{CachePrefix}-documento-{id}";
            var result = await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Documento(id);
            });

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public virtual async Task<List<DocumentoRelacionadoDTO>> DocumentosDoMesmoDia(int id)
        {
            var cacheKey = $"{CachePrefix}-documentos-mesmo-dia-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.DocumentosDoMesmoDia(id);
            });
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public virtual async Task<List<DocumentoRelacionadoDTO>> DocumentosDaSubcotaMes(int id)
        {
            var cacheKey = $"{CachePrefix}-documentos-subcota-mes-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.DocumentosDaSubcotaMes(id);
            });
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public virtual async Task<List<ParlamentarNotaDTO>> MaioresNotas(int id)
        {
            var cacheKey = $"{CachePrefix}-maiores-notas-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.MaioresNotas(id);
            });
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public virtual async Task<List<DeputadoFornecedorDTO>> MaioresFornecedores(int id)
        {
            var cacheKey = $"{CachePrefix}-maiores-fornecedores-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.MaioresFornecedores(id);
            });
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public virtual async Task<GraficoBarraDTO> GastosPorAno(int id)
        {
            var cacheKey = $"{CachePrefix}-gastos-por-ano-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.GastosPorAno(id);
            });
        }

        [HttpGet("{id:int}/CustoAnual")]
        public virtual async Task<List<ParlamentarCustoAnualDTO>> CustoAnual(int id)
        {
            var cacheKey = $"{CachePrefix}-custo-anual-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.CustoAnual(id);
            });
        }
    }
}
