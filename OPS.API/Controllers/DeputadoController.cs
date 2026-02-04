using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.API.Services;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("deputado-federal")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoController : ParlamentarBaseController<DeputadoRepository>
    {
        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }

        public DeputadoController(IConfiguration configuration, IWebHostEnvironment env, DeputadoRepository deputadoRepository, IHybridCacheService hybridCacheService)
            : base(deputadoRepository, hybridCacheService)
        {
            Environment = env;
            Configuration = configuration;
        }

        protected override string CachePrefix => "deputado";

        [HttpGet("{id:int}/GastosComPessoalPorAno")]
        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            var cacheKey = $"deputado-gastos-pessoal-por-ano-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.GastosComPessoalPorAno(id);
            });
        }

        [HttpPost("FuncionarioPesquisa")]
        public async Task<dynamic> FuncionarioPesquisa(MultiSelectRequest filtro)
        {
            var cacheKey = $"deputado-funcionario-pesquisa-{JsonSerializer.Serialize(filtro)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.FuncionarioPesquisa(filtro);
            });
        }

        [HttpPost("Funcionarios")]
        public async Task<dynamic> Funcionarios(DataTablesRequest request)
        {
            var cacheKey = $"deputado-funcionarios-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Funcionarios(request);
            });
        }

        [HttpPost("{id:int}/FuncionariosAtivos")]
        public async Task<dynamic> FuncionariosAtivosPorDeputado(int id, DataTablesRequest request)
        {
            var cacheKey = $"deputado-funcionarios-ativos-{id}-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.FuncionariosAtivosPorDeputado(id, request);
            });
        }

        [HttpPost("{id:int}/FuncionariosHistorico")]
        public async Task<dynamic> FuncionariosPorDeputado(int id, DataTablesRequest request)
        {
            var cacheKey = $"deputado-funcionarios-historico-{id}-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.FuncionariosPorDeputado(id, request);
            });
        }

        [HttpGet("{id:int}/ResumoPresenca")]
        public async Task<dynamic> ResumoPresenca(int id)
        {
            var cacheKey = $"deputado-resumo-presenca-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.ResumoPresenca(id);
            });
        }

        [HttpGet("ResumoMensal")]
        public async Task<dynamic> CamaraResumoMensal()
        {
            const string cacheKey = "deputado-camara-resumo-mensal";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.CamaraResumoMensal();
            });
        }

        [HttpGet("ResumoAnual")]
        public async Task<dynamic> CamaraResumoAnual()
        {
            const string cacheKey = "deputado-camara-resumo-anual";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.CamaraResumoAnual();
            });
        }

        [HttpPost("Frequencia/{id:int}")]
        public async Task<dynamic> Frequencia(int id, DataTablesRequest request)
        {
            var cacheKey = $"deputado-frequencia-{id}-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Frequencia(id, request);
            });
        }

        [HttpPost("Frequencia")]
        public async Task<dynamic> Frequencia(DataTablesRequest request)
        {
            var cacheKey = $"deputado-frequencia-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Frequencia(request);
            });
        }

        [HttpGet]
        [Route("GrupoFuncional")]
        public async Task<dynamic> GrupoFuncional()
        {
            const string cacheKey = "deputado-grupo-funcional";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.GrupoFuncional();
            });
        }

        [HttpGet]
        [Route("Cargo")]
        public async Task<dynamic> Cargo()
        {
            const string cacheKey = "deputado-cargo";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Cargo();
            });
        }

        [HttpPost]
        [Route("Remuneracao")]
        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            var cacheKey = $"deputado-remuneracao-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Remuneracao(request);
            });
        }

        [HttpGet]
        [Route("Remuneracao/{id:int}")]
        public async Task<dynamic> Remuneracao(int id)
        {
            var cacheKey = $"deputado-remuneracao-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Remuneracao(id);
            });
        }
    }
}
