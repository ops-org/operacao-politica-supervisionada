using OPS.Core;
using OPS.Dao;
using OPS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace OPS.WebApi
{
    public class FornecedorController : ApiController
    {
		FornecedorDao dao;

		public FornecedorController()
		{
			dao = new FornecedorDao();
		}

		[HttpGet]
		[ActionName("Get")]
		public Fornecedor Consulta(string value)
		{
			return dao.Consulta(value);
		}

		[HttpGet]
		public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		{
			return dao.Pesquisa(filtro);
		}

		[HttpGet]
		public List<FornecedorQuadroSocietario> QuadroSocietario(string value)
		{
			return dao.QuadroSocietario(value);
		}

		private const string urlBaseReceitaFederal = "http://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/";
		private const string paginaValidacao = "valida.asp";
		private const string paginaPrincipal = "cnpjreva_solicitacao2.asp";
		private const string paginaCaptcha = "captcha/gerarCaptcha.asp";
		private const string paginaQuadroSocietario = "Cnpjreva_qsa.asp";

		[HttpGet]
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
		public dynamic ConsultarDadosCnpj(Newtonsoft.Json.Linq.JObject jsonData)
		{
			var msg = string.Empty;
			Fornecedor fornecedor = ObterDados(jsonData["cnpj"].ToString(), jsonData["captcha"].ToString());

			if (fornecedor != null)
			{
				if (fornecedor.AtividadeSecundaria02 != "" && fornecedor.AtividadeSecundaria02 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria02;

				if (fornecedor.AtividadeSecundaria03 != "" && fornecedor.AtividadeSecundaria03 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria03;

				if (fornecedor.AtividadeSecundaria04 != "" && fornecedor.AtividadeSecundaria04 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria04;

				if (fornecedor.AtividadeSecundaria05 != "" && fornecedor.AtividadeSecundaria05 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria05;

				if (fornecedor.AtividadeSecundaria06 != "" && fornecedor.AtividadeSecundaria06 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria06;

				if (fornecedor.AtividadeSecundaria07 != "" && fornecedor.AtividadeSecundaria07 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria07;

				if (fornecedor.AtividadeSecundaria08 != "" && fornecedor.AtividadeSecundaria08 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria08;

				if (fornecedor.AtividadeSecundaria09 != "" && fornecedor.AtividadeSecundaria09 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria09;

				if (fornecedor.AtividadeSecundaria10 != "" && fornecedor.AtividadeSecundaria10 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria10;

				if (fornecedor.AtividadeSecundaria11 != "" && fornecedor.AtividadeSecundaria11 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria11;

				if (fornecedor.AtividadeSecundaria12 != "" && fornecedor.AtividadeSecundaria12 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria12;

				if (fornecedor.AtividadeSecundaria13 != "" && fornecedor.AtividadeSecundaria13 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria13;

				if (fornecedor.AtividadeSecundaria14 != "" && fornecedor.AtividadeSecundaria14 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria14;

				if (fornecedor.AtividadeSecundaria15 != "" && fornecedor.AtividadeSecundaria15 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria15;

				if (fornecedor.AtividadeSecundaria16 != "" && fornecedor.AtividadeSecundaria16 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria16;

				if (fornecedor.AtividadeSecundaria17 != "" && fornecedor.AtividadeSecundaria17 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria17;

				if (fornecedor.AtividadeSecundaria18 != "" && fornecedor.AtividadeSecundaria18 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria18;

				if (fornecedor.AtividadeSecundaria19 != "" && fornecedor.AtividadeSecundaria19 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria19;

				if (fornecedor.AtividadeSecundaria20 != "" && fornecedor.AtividadeSecundaria20 != null)
					fornecedor.AtividadeSecundaria01 += "<br />" + fornecedor.AtividadeSecundaria20;
			}

			return
				new
				{
					erro = msg,
					dados = fornecedor
				};
		}

		private static Fornecedor ObterDados(string aCNPJ, string aCaptcha)
		{
			string cnpj = new Regex(@"[^\d]").Replace(aCNPJ, string.Empty);
			CookieContainer _cookies = CacheHelper.Get<CookieContainer>("CookieReceitaFederal_" + aCNPJ);

			var request = (HttpWebRequest)WebRequest.Create(urlBaseReceitaFederal + paginaValidacao);
			request.ProtocolVersion = HttpVersion.Version10;
			request.CookieContainer = _cookies;
			request.Method = "POST";

			var postData = string.Empty;
			postData += "origem=comprovante&";
			postData += "cnpj=" + cnpj + "&";
			postData += "txtTexto_captcha_serpro_gov_br=" + aCaptcha + "&";
			postData += "submit1=Consultar&";
			postData += "search_type=cnpj";

			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteArray.Length;

			var dataStream = request.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();

			var response = request.GetResponse();
			var stHtml = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));
			var strHtmlFornecedor = stHtml.ReadToEnd();

			if (strHtmlFornecedor.Contains("Verifique se o mesmo foi digitado corretamente"))
				throw new Exception("O número do CNPJ não foi localizado na Receita Federal");

			if (strHtmlFornecedor.Contains("Erro na Consulta"))
				throw new Exception("Os caracteres não conferem com a imagem");

			if (strHtmlFornecedor.Length > 0)
			{
				var requestQAS = (HttpWebRequest)HttpWebRequest.Create(urlBaseReceitaFederal + paginaQuadroSocietario);
				requestQAS.CookieContainer = _cookies;
				requestQAS.Method = "GET";
				var res = (HttpWebResponse)requestQAS.GetResponse();
				StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));
				string strHtmlQuadroSocietario = sr.ReadToEnd();

				var formataDados = new FormatarDados();
				Fornecedor fornecedor = formataDados.MontarObjFornecedor(cnpj, strHtmlFornecedor);
				if (fornecedor != null)
				{
					formataDados.MontarObjFornecedorQuadroSocietario(fornecedor, strHtmlQuadroSocietario);

					string UserName;
					try
					{
						UserName = HttpContext.Current.User.Identity.Name;
					}
					catch (Exception)
					{
						UserName = "anonymous";
					}

					fornecedor.UsuarioInclusao = UserName;
					fornecedor.DataInclusao = DateTime.Now.ToString();


					var fornecedorDao = new FornecedorDao();
					fornecedorDao.AtualizaDados(fornecedor);

					fornecedorDao.MarcaVisitado(fornecedor.CnpjCpf, UserName);
				}

				return fornecedor;
			}

			return null;
		}

		[HttpGet]
		public dynamic RecebimentosMensaisPorAno(string value)
		{
			return dao.RecebimentosMensaisPorAno(value);
		}

		[HttpGet]
		public dynamic DeputadoFederalMaioresGastos(string value)
		{
			return dao.DeputadoFederalMaioresGastos(value);
		}

		[HttpGet]
		public dynamic SenadoresMaioresGastos(string value)
		{
			return dao.SenadoresMaioresGastos(value);
		}
	}
}
