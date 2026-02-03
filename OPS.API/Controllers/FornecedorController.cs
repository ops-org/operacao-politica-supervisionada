using System;
using System.Collections.Generic;
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
    public class FornecedorController : Controller
    {
        public FornecedorRepository dao { get; }
        private readonly HybridCache _hybridCache;

        public FornecedorController(FornecedorRepository fornecedorRepository, HybridCache hybridCache)
        {
            dao = fornecedorRepository;
            _hybridCache = hybridCache;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<FornecedorDetalheDTO> Consulta(int id)
        {
            var cacheKey = $"fornecedor-consulta-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                var _fornecedor = await dao.Consulta(id);
                var _quadro_societario = await dao.QuadroSocietario(id);

                return new FornecedorDetalheDTO
                {
                    Fornecedor = _fornecedor,
                    QuadroSocietario = _quadro_societario
                };
            });
        }

        [HttpPost]
        [Route("Consulta")]
        public async Task<List<FornecedorListaDTO>> Consulta(Dictionary<string, string> jsonData)
        {
            if (jsonData == null) throw new ArgumentNullException(nameof(jsonData));

            string cnpj = "", nome = "";
            if (jsonData.ContainsKey("cnpj")) cnpj = jsonData["cnpj"];
            if (jsonData.ContainsKey("nome")) nome = jsonData["nome"];

            var cacheKey = $"fornecedor-consulta-{JsonSerializer.Serialize(jsonData)}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return dao.Consulta(cnpj, nome);
            });
        }


        [HttpGet]
        [Route("{id:int}/RecebimentosPorAno")]
        public async Task<GraficoBarraDTO> RecebimentosPorAno(int id)
        {
            var cacheKey = $"fornecedor-recebimentos-por-ano-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await dao.RecebimentosPorAno(id);
            });
        }

        [HttpGet]
        [Route("{id:int}/MaioresGastos")]
        public async Task<object> MaioresGastos(int id)
        {
            var cacheKey = $"fornecedor-maiores-gastos-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await dao.MaioresGastos(id);
            });
        }

    }
}
