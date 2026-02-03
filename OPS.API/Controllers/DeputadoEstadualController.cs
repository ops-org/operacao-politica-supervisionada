using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.API.Services;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("deputado-estadual")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoEstadualController : ParlamentarBaseController<DeputadoEstadualRepository>
    {
        public DeputadoEstadualController(DeputadoEstadualRepository deputadoEstadualRepository, IHybridCacheService hybridCacheService)
            : base(deputadoEstadualRepository, hybridCacheService)
        {
        }

        protected override string CachePrefix => "deputado-estadual";

        [HttpGet("ResumoMensal")]
        public async Task<dynamic> ResumoMensal()
        {
            const string cacheKey = "deputado-estadual-resumo-mensal";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.ResumoMensal();
            });
        }

        [HttpGet("ResumoAnual")]
        public async Task<dynamic> ResumoAnual()
        {
            const string cacheKey = "deputado-estadual-resumo-anual";
            return await _hybridCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _repository.ResumoAnual();
            });
        }
    }
}
