using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core;
using OPS.Core.DAO;
using OPS.Core.Models;
using System;
using System.Net;

namespace OPS.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class FornecedorController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public FornecedorDao dao { get; }

        public FornecedorController(IConfiguration configuration, IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = configuration;

            dao = new FornecedorDao();
        }

        [HttpGet]
        [Route("{id:int}")]
        [IgnoreCacheOutput]
        public dynamic Consulta(int id)
        {
            var _fornecedor = dao.Consulta(id);
            var _quadro_societario = dao.QuadroSocietario(id);

            return new { fornecedor = _fornecedor, quadro_societario = _quadro_societario };

        }

        [HttpPost]
        [Route("Consulta")]
        public dynamic Consulta(Newtonsoft.Json.Linq.JObject jsonData)
        {
            return dao.Consulta(Convert.ToString(jsonData["cnpj"]), Convert.ToString(jsonData["nome"])); ;
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
        [Route("{id:int}/RecebimentosMensaisPorAnoDeputados")]
        public dynamic RecebimentosMensaisPorAnoDeputados(int id)
        {
            return dao.RecebimentosMensaisPorAnoDeputados(id);
        }

        [HttpGet]
        [Route("{id:int}/RecebimentosMensaisPorAnoSenadores")]
        public dynamic RecebimentosMensaisPorAnoSenadores(int id)
        {
            return dao.RecebimentosMensaisPorAnoSenadores(id);
        }

        [HttpGet]
        [Route("{id:int}/DeputadoFederalMaioresGastos")]
        public dynamic DeputadoFederalMaioresGastos(int id)
        {
            return dao.DeputadoFederalMaioresGastos(id);
        }

        [HttpGet]
        [Route("{id:int}/SenadoresMaioresGastos")]
        public dynamic SenadoresMaioresGastos(int id)
        {
            return dao.SenadoresMaioresGastos(id);
        }

        private const string urlBaseReceitaFederal = "http://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/";
        private const string paginaValidacao = "valida.asp";
        private const string paginaPrincipal = "Cnpjreva_solicitacao3.asp";
        private const string paginaCaptcha = "captcha/gerarCaptcha.asp";
        private const string paginaQuadroSocietario = "Cnpjreva_qsa.asp";

        [HttpGet]
        [IgnoreCacheOutput]
        [Route("Captcha/{value}")]
        public string Captcha(string value)
        {
            CookieContainer _cookies = new CookieContainer();
            var htmlResult = string.Empty;

            using (var wc = new CookieAwareWebClient(_cookies))
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; Synapse)";
                wc.Headers[HttpRequestHeader.KeepAlive] = "300";
                htmlResult = wc.DownloadString(urlBaseReceitaFederal + paginaPrincipal);
            }

            if (htmlResult.Length > 0)
            {
                var wc2 = new CookieAwareWebClient(_cookies);
                wc2.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; Synapse)";
                wc2.Headers[HttpRequestHeader.KeepAlive] = "300";
                byte[] data = wc2.DownloadData(urlBaseReceitaFederal + paginaCaptcha);

                CacheHelper.Add("CookieReceitaFederal_" + value, _cookies);

                return "data:image/jpeg;base64," + System.Convert.ToBase64String(data, 0, data.Length);
            }

            return string.Empty;
        }

        [HttpPost]
        [IgnoreCacheOutput, InvalidateCacheOutput(nameof(Consulta))]
        [Route("ConsultarDadosCnpj")]
        public dynamic ConsultarDadosCnpj(Newtonsoft.Json.Linq.JObject jsonData)
        {
            string aCNPJ = jsonData["cnpj"].ToString();
            CookieContainer _cookies = CacheHelper.Get<CookieContainer>("CookieReceitaFederal_" + aCNPJ);

            var oFormatarDados = new FormatarDados();
            var fornecedor = oFormatarDados.ObterDados(_cookies, aCNPJ, jsonData["captcha"].ToString());

            // Testar
            ////now get cache instance
            //var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            ////and invalidate cache for method "Get" of "FornecedorController"
            //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey("FornecedorController", "Get"));

            if (fornecedor != null) //encontrou?
            {
                var _fornecedor = dao.Consulta(fornecedor.id);
                var _quadro_societario = dao.QuadroSocietario(fornecedor.id);

                return new { fornecedor = _fornecedor, quadro_societario = _quadro_societario };
            }
            else
            {
                return null;
            }
        }
    }
}
