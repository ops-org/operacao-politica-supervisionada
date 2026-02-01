using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class FornecedorController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public FornecedorRepository dao { get; }
        private readonly HybridCache _hybridCache;

        public FornecedorController(IConfiguration configuration, IWebHostEnvironment env, FornecedorRepository fornecedorRepository, HybridCache hybridCache)
        {
            Environment = env;
            Configuration = configuration;
            dao = fornecedorRepository;
            _hybridCache = hybridCache;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<dynamic> Consulta(int id)
        {
            var cacheKey = $"fornecedor-consulta-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                var _fornecedor = await dao.Consulta(id);
                var _quadro_societario = await dao.QuadroSocietario(id);

                return new { fornecedor = _fornecedor, quadro_societario = _quadro_societario };
            });
        }

        [HttpPost]
        [Route("Consulta")]
        public async Task<dynamic> Consulta(Dictionary<string, string> jsonData)
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

        //[HttpGet]
        //public dynamic Pesquisa( FiltroDropDownDTO filtro)
        //{
        //	return dao.Pesquisa(filtro);
        //}

        //[HttpGet]
        //public dynamic QuadroSocietario(int id)
        //{
        //	return dao.QuadroSocietario(id);
        //}

        [HttpGet]
        [Route("{id:int}/RecebimentosPorAno")]
        public async Task<dynamic> RecebimentosPorAno(int id)
        {
            var cacheKey = $"fornecedor-recebimentos-por-ano-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await dao.RecebimentosPorAno(id);
            });
        }

        [HttpGet]
        [Route("{id:int}/MaioresGastos")]
        public async Task<dynamic> MaioresGastos(int id)
        {
            var cacheKey = $"fornecedor-maiores-gastos-{id}";
            return await _hybridCache.GetOrCreateAsync(cacheKey, async _ =>
            {
                return await dao.MaioresGastos(id);
            });
        }

        private const string urlBaseReceitaFederal = "http://servicos.receita.fazenda.gov.br/Servicos/cnpjreva/";
        //private const string paginaValidacao = "valida.asp";
        private const string paginaPrincipal = "Cnpjreva_Solicitacao_CS.asp";
        private const string paginaCaptcha = "captcha/gerarCaptcha.asp";
        //private const string paginaQuadroSocietario = "Cnpjreva_qsa.asp";

        //[HttpGet]
        ////[IgnoreCacheOutput]
        //[Route("Captcha/{value}")]
        //public string Captcha(string value)
        //{
        //    CookieContainer _cookies = new CookieContainer();
        //    var htmlResult = string.Empty;

        //    using (var wc = new CookieAwareWebClient(_cookies))
        //    {
        //        wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; Synapse)";
        //        wc.Headers[HttpRequestHeader.KeepAlive] = "300";
        //        htmlResult = wc.DownloadString(urlBaseReceitaFederal + paginaPrincipal);
        //    }

        //    if (htmlResult.Length > 0)
        //    {
        //        using (var wc2 = new CookieAwareWebClient(_cookies))
        //        {
        //            wc2.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; Synapse)";
        //            wc2.Headers[HttpRequestHeader.KeepAlive] = "300";
        //            byte[] data = wc2.DownloadData(urlBaseReceitaFederal + paginaCaptcha);

        //            CacheHelper.Add("CookieReceitaFederal_" + value, _cookies);

        //            return "data:image/jpeg;base64," + Convert.ToBase64String(data, 0, data.Length);
        //        }
        //    }

        //    return string.Empty;
        //}

        //[HttpPost]
        ////[IgnoreCacheOutput, InvalidateCacheOutput(nameof(Consulta))]
        //[Route("ConsultarDadosCnpj")]
        //public dynamic ConsultarDadosCnpj(JsonDocument jsonData)
        //{
        //    if (jsonData == null) throw new ArgumentNullException(nameof(jsonData));

        //    JsonElement root = jsonData.RootElement;

        //    string aCNPJ = root.GetProperty("cnpj").ToString();
        //    CookieContainer _cookies = CacheHelper.Get<CookieContainer>("CookieReceitaFederal_" + aCNPJ);

        //    var oFormatarDados = new FormatarDados();
        //    var fornecedor = oFormatarDados.ObterDados(_cookies, aCNPJ, root.GetProperty("captcha").ToString());

        //    // Testar
        //    ////now get cache instance
        //    //var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
        //    ////and invalidate cache for method "Get" of "FornecedorController"
        //    //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey("FornecedorController", "Get"));

        //    if (fornecedor != null) //encontrou?
        //    {
        //        var _fornecedor = dao.Consulta(fornecedor.id);
        //        var _quadro_societario = dao.QuadroSocietario(fornecedor.id);

        //        return new { fornecedor = _fornecedor, quadro_societario = _quadro_societario };
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}
