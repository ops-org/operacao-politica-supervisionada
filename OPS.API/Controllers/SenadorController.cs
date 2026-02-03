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
    public class SenadorController : ParlamentarBaseController<SenadorRepository>
    {
        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }

        public SenadorController(IConfiguration configuration, IWebHostEnvironment env, SenadorRepository senadorRepository, IHybridCacheService hybridCacheService)
            : base(senadorRepository, hybridCacheService)
        {
            Environment = env;
            Configuration = configuration;
        }

        protected override string CachePrefix => "senador";

        [HttpGet("{id:int}/GastosComPessoalPorAno")]
        public async Task<GraficoBarraDTO> GastosComPessoalPorAno(int id)
        {
            var cacheKey = $"senador-gastos-pessoal-por-ano-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.GastosComPessoalPorAno(id);
            });
        }

        [HttpGet("ResumoMensal")]
        public async Task<object> SenadoResumoMensal()
        {
            const string cacheKey = "senador-senado-resumo-mensal";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.SenadoResumoMensal();
            });
        }

        [HttpGet("ResumoAnual")]
        public async Task<object> SenadoResumoAnual()
        {
            const string cacheKey = "senador-senado-resumo-anual";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.SenadoResumoAnual();
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

        [HttpGet("Lotacao")]
        public async Task<object> Lotacao()
        {
            const string cacheKey = "senador-lotacao";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Lotacao();
            });
        }

        [HttpGet("Cargo")]
        public async Task<object> Cargo()
        {
            const string cacheKey = "senador-cargo";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Cargo();
            });
        }

        [HttpGet("Categoria")]
        public async Task<object> Categoria()
        {
            const string cacheKey = "senador-categoria";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Categoria();
            });
        }

        [HttpGet("Vinculo")]
        public async Task<object> Vinculo()
        {
            const string cacheKey = "senador-vinculo";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Vinculo();
            });
        }

        [HttpPost("Remuneracao")]
        public async Task<object> Remuneracao(DataTablesRequest request)
        {
            var cacheKey = $"senador-remuneracao-{JsonSerializer.Serialize(request)}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Remuneracao(request);
            });
        }

        [HttpGet("Remuneracao/{id:int}")]
        public async Task<object> Remuneracao(int id)
        {
            var cacheKey = $"senador-remuneracao-{id}";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.Remuneracao(id);
            });
        }
    }
}
