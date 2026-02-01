using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SenadorController : Controller
    {

        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }
        private readonly SenadorRepository _senadorRepository;
        private readonly HybridCache _hybridCache;

        public SenadorController(IConfiguration configuration, IWebHostEnvironment env, SenadorRepository senadorRepository, HybridCache hybridCache)
        {
            Environment = env;
            Configuration = configuration;
            _senadorRepository = senadorRepository;
            _hybridCache = hybridCache;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            var cacheKey = $"senador-consultar-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.Consultar(id);
            });
        }

        [HttpPost("Lista")]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            var cacheKey = $"senador-lista-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.Lista(filtro);
            });
        }


        [HttpPost("Pesquisa")]
        public async Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro)
        {
            var cacheKey = $"senador-pesquisa-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.Pesquisa(filtro);
            });
        }

        [HttpPost]
        [Route("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            var cacheKey = $"senador-lancamentos-{JsonSerializer.Serialize(request)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.Lancamentos(request);
            });
        }

        [HttpGet]
        [Route("TipoDespesa")]
        public async Task<dynamic> TipoDespesa()
        {
            const string cacheKey = "senador-tipo-despesa";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.TipoDespesa();
            });
        }

        [HttpGet("Documento/{id:int}")]
        public async Task<ActionResult<DocumentoDetalheDTO>> Documento(int id)
        {
            var cacheKey = $"senador-documento-{id}";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.Documento(id);
            });

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            var cacheKey = $"senador-documentos-mesmo-dia-{id}";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.DocumentosDoMesmoDia(id);
            });

            return Ok(result);
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            var cacheKey = $"senador-documentos-subcota-mes-{id}";
            var result = await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.DocumentosDaSubcotaMes(id);
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            var cacheKey = $"senador-gastos-por-ano-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.GastosPorAno(id);
            });
        }

        [HttpGet("{id:int}/GastosComPessoalPorAno")]
        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            var cacheKey = $"senador-gastos-pessoal-por-ano-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.GastosComPessoalPorAno(id);
            });
        }

        [HttpGet]
        [Route("{id:int}/CustoAnual")]
        public async Task<dynamic> CustoAnual(int id)
        {
            var cacheKey = $"senador-custo-anual-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.CustoAnual(id);
            });
        }

        [HttpGet]
        [Route("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            var cacheKey = $"senador-maiores-notas-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.MaioresNotas(id);
            });
        }

        [HttpGet]
        [Route("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            var cacheKey = $"senador-maiores-fornecedores-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.MaioresFornecedores(id);
            });
        }

        [HttpGet]
        [Route("SenadoResumoMensal")]
        public async Task<dynamic> SenadoResumoMensal()
        {
            const string cacheKey = "senador-senado-resumo-mensal";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.SenadoResumoMensal();
            });
        }

        [HttpGet]
        [Route("SenadoResumoAnual")]
        public async Task<dynamic> SenadoResumoAnual()
        {
            const string cacheKey = "senador-senado-resumo-anual";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.SenadoResumoAnual();
            });
        }

        [HttpGet("Imagem/{id}")]
        public VirtualFileResult Imagem(string id)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
            {
                var file = @"images/senador/" + id + ".jpg";
                var filePath = System.IO.Path.Combine(Environment.ContentRootPath, file);

                if (System.IO.File.Exists(filePath))
                {
                    return File(file, "image/jpeg");
                }
            }

            return File(@"images/sem_foto.jpg", "image/jpeg");
        }

        [HttpGet]
        [Route("Lotacao")]
        public async Task<dynamic> Lotacao()
        {
            const string cacheKey = "senador-lotacao";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.Lotacao();
            });
        }

        [HttpGet]
        [Route("Cargo")]
        public async Task<dynamic> Cargo()
        {
            const string cacheKey = "senador-cargo";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.Cargo();
            });
        }

        [HttpGet]
        [Route("Categoria")]
        public async Task<dynamic> Categoria()
        {
            const string cacheKey = "senador-categoria";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.Categoria();
            });
        }

        [HttpGet]
        [Route("Vinculo")]
        public async Task<dynamic> Vinculo()
        {
            const string cacheKey = "senador-vinculo";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async token =>
            {
                return await _senadorRepository.Vinculo();
            });
        }

        [HttpPost]
        [Route("Remuneracao")]
        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            var cacheKey = $"senador-remuneracao-{JsonSerializer.Serialize(request)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.Remuneracao(request);
            });
        }

        [HttpGet]
        [Route("Remuneracao/{id:int}")]
        public async Task<dynamic> Remuneracao(int id)
        {
            var cacheKey = $"senador-remuneracao-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await _senadorRepository.Remuneracao(id);
            });
        }
    }
}
