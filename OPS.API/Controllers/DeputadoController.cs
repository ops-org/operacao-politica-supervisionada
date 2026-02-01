using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using OPS.API.Extensions;
using OPS.API.Services;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("deputado-federal")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }
        private readonly DeputadoRepository _deputadoRepository;
        private readonly IHybridCacheService _hybridCacheService;

        public DeputadoController(IConfiguration configuration, IWebHostEnvironment env, DeputadoRepository deputadoRepository, IHybridCacheService hybridCacheService)
        {
            Environment = env;
            Configuration = configuration;
            _deputadoRepository = deputadoRepository;
            _hybridCacheService = hybridCacheService;
        }

        [HttpGet("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            var cacheKey = $"deputado-consultar-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Consultar(id);
            });
        }

        [HttpPost("Lista")]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            var cacheKey = $"deputado-lista-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Lista(filtro);
            });
        }

        [HttpPost("Pesquisa")]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            var cacheKey = $"deputado-pesquisa-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Pesquisa(filtro);
            });
        }

        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            var cacheKey = $"deputado-lancamentos-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Lancamentos(request);
            });
        }

        [HttpGet("TipoDespesa")]
        public async Task<dynamic> TipoDespesa()
        {
            const string cacheKey = "deputado-tipo-despesa";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.TipoDespesa();
            });
        }

        [HttpPost("FuncionarioPesquisa")]
        public async Task<dynamic> FuncionarioPesquisa(MultiSelectRequest filtro)
        {
            var cacheKey = $"deputado-funcionario-pesquisa-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.FuncionarioPesquisa(filtro);
            });
        }

        [HttpPost("Funcionarios")]
        public async Task<dynamic> Funcionarios(DataTablesRequest request)
        {
            var cacheKey = $"deputado-funcionarios-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Funcionarios(request);
            });
        }

        [HttpPost("{id:int}/FuncionariosAtivos")]
        public async Task<dynamic> FuncionariosAtivosPorDeputado(int id, DataTablesRequest request)
        {
            var cacheKey = $"deputado-funcionarios-ativos-{id}-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.FuncionariosAtivosPorDeputado(id, request);
            });
        }

        [HttpPost("{id:int}/FuncionariosHistorico")]
        public async Task<dynamic> FuncionariosPorDeputado(int id, DataTablesRequest request)
        {
            var cacheKey = $"deputado-funcionarios-historico-{id}-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.FuncionariosPorDeputado(id, request);
            });
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            var cacheKey = $"deputado-gastos-por-ano-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.GastosPorAno(id);
            });
        }

        [HttpGet("{id:int}/GastosComPessoalPorAno")]
        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            var cacheKey = $"deputado-gastos-pessoal-por-ano-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.GastosComPessoalPorAno(id);
            });
        }

        [HttpGet("Documento/{id:int}")]
        public async Task<ActionResult<DocumentoDetalheDTO>> Documento(int id)
        {
            var cacheKey = $"deputado-documento-{id}";
            var result = await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Documento(id);
            });

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            var cacheKey = $"deputado-documentos-mesmo-dia-{id}";
            var result = await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.DocumentosDoMesmoDia(id);
            });

            return Ok(result);
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            var cacheKey = $"deputado-documentos-subcota-mes-{id}";
            var result = await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.DocumentosDaSubcotaMes(id);
            });

            return Ok(result);
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            var cacheKey = $"deputado-maiores-notas-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.MaioresNotas(id);
            });
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            var cacheKey = $"deputado-maiores-fornecedores-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.MaioresFornecedores(id);
            });
        }

        [HttpGet("{id:int}/ResumoPresenca")]
        public async Task<dynamic> ResumoPresenca(int id)
        {
            var cacheKey = $"deputado-resumo-presenca-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.ResumoPresenca(id);
            });
        }

        [HttpGet("CamaraResumoMensal")]
        public async Task<dynamic> CamaraResumoMensal()
        {
            const string cacheKey = "deputado-camara-resumo-mensal";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.CamaraResumoMensal();
            });
        }

        [HttpGet("CamaraResumoAnual")]
        public async Task<dynamic> CamaraResumoAnual()
        {
            const string cacheKey = "deputado-camara-resumo-anual";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.CamaraResumoAnual();
            });
        }

        [HttpPost("Frequencia/{id:int}")]
        public async Task<dynamic> Frequencia(int id, DataTablesRequest request)
        {
            var cacheKey = $"deputado-frequencia-{id}-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Frequencia(id, request);
            });
        }

        [HttpPost("Frequencia")]
        public async Task<dynamic> Frequencia(DataTablesRequest request)
        {
            var cacheKey = $"deputado-frequencia-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Frequencia(request);
            });
        }

        [HttpGet("Imagem/{id}")]
        public VirtualFileResult Imagem(string id)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
            {
                var file = @"images/depfederal/" + id + ".jpg";
                var filePath = System.IO.Path.Combine(Environment.ContentRootPath, file);

                if (System.IO.File.Exists(filePath))
                {
                    return File(file, "image/jpeg");
                }
            }

            return File(@"images/sem_foto.jpg", "image/jpeg");
        }

        [HttpGet]
        [Route("GrupoFuncional")]
        public async Task<dynamic> GrupoFuncional()
        {
            const string cacheKey = "deputado-grupo-funcional";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.GrupoFuncional();
            });
        }

        [HttpGet]
        [Route("Cargo")]
        public async Task<dynamic> Cargo()
        {
            const string cacheKey = "deputado-cargo";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Cargo();
            });
        }

        [HttpPost]
        [Route("Remuneracao")]
        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            var cacheKey = $"deputado-remuneracao-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Remuneracao(request);
            });
        }

        [HttpGet]
        [Route("Remuneracao/{id:int}")]
        public async Task<dynamic> Remuneracao(int id)
        {
            var cacheKey = $"deputado-remuneracao-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.Remuneracao(id);
            });
        }

        [HttpGet]
        [Route("{id:int}/CustoAnual")]
        public async Task<dynamic> CustoAnual(int id)
        {
            var cacheKey = $"deputado-custo-anual-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _deputadoRepository.CustoAnual(id);
            });
        }
    }
}
