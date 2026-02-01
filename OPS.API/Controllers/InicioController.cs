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
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class InicioController : Controller
    {
        private readonly DeputadoRepository _deputadoRepository;
        private readonly DeputadoEstadualRepository _deputadoEstadualRepository;
        private readonly SenadorRepository _senadorRepository;
        private readonly FornecedorRepository _fornecedorRepository;
        private readonly InicioRepository _inicioRepository;
        private readonly HybridCache _hybridCache;

        public InicioController(
            DeputadoRepository deputadoRepository,
            DeputadoEstadualRepository deputadoEstadualRepository,
            SenadorRepository senadorRepository,
            FornecedorRepository fornecedorRepository,
            InicioRepository inicioRepository,
            HybridCache hybridCache)
        {
            _deputadoRepository = deputadoRepository;
            _deputadoEstadualRepository = deputadoEstadualRepository;
            _senadorRepository = senadorRepository;
            _fornecedorRepository = fornecedorRepository;
            _inicioRepository = inicioRepository;
            _hybridCache = hybridCache;
        }

        [HttpGet]
        [Route("ParlamentarResumoGastos")]
        public async Task<dynamic> ParlamentarResumoGastos()
        {
            const string cacheKey = "inicio-parlamentar-resumo-gastos";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _inicioRepository.ParlamentarResumoGastos();
            });
        }

        [HttpGet]
        [Route("Busca")]
        public async Task<dynamic> Busca([FromQuery] string value)
        {
            var cacheKey = $"inicio-busca-{value}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                var filtro = new FiltroParlamentarDTO() { NomeParlamentar = value };

                return new
                {
                    deputado_federal = await _deputadoRepository.Lista(filtro),
                    deputado_estadual = await _deputadoEstadualRepository.Lista(filtro),
                    senador = await _senadorRepository.Lista(filtro),
                    fornecedor = await _fornecedorRepository.Busca(value)
                };
            });
        }

        [HttpGet]
        [Route("Importacao")]
        public async Task<dynamic> Importacao()
        {
            const string cacheKey = "inicio-importacao";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _inicioRepository.InfoImportacao();
            });
        }
    }
}
