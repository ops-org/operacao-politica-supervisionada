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
		public dynamic Consulta(int id)
		{
			var _fornecedor = dao.Consulta(id);
			var _quadro_societario = dao.QuadroSocietario(id);

			return new { fornecedor = _fornecedor, quadro_societario = _quadro_societario };

		}

		//[HttpGet]
		//public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		//{
		//	return dao.Pesquisa(filtro);
		//}

		//[HttpGet]
		//public dynamic QuadroSocietario(int id)
		//{
		//	return dao.QuadroSocietario(id);
		//}

		[HttpGet]
		public dynamic RecebimentosMensaisPorAnoDeputados(int id)
		{
			return dao.RecebimentosMensaisPorAnoDeputados(id);
		}

		[HttpGet]
		public dynamic RecebimentosMensaisPorAnoSenadores(int id)
		{
			return dao.RecebimentosMensaisPorAnoSenadores(id);
		}

		[HttpGet]
		public dynamic DeputadoFederalMaioresGastos(int id)
		{
			return dao.DeputadoFederalMaioresGastos(id);
		}

		[HttpGet]
		public dynamic SenadoresMaioresGastos(int id)
		{
			return dao.SenadoresMaioresGastos(id);
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
			return ObterDados(jsonData["cnpj"].ToString(), jsonData["captcha"].ToString());
		}

		private dynamic ObterDados(string aCNPJ, string aCaptcha)
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

					//string UserName;
					//try
					//{
					//	UserName = HttpContext.Current.User.Identity.Name;
					//}
					//catch (Exception)
					//{
					//	UserName = "anonymous";
					//}

					//fornecedor.UsuarioInclusao = UserName;
					//fornecedor.DataInclusao = DateTime.Now.ToString();

					var fornecedorDao = new FornecedorDao();
					var id = fornecedorDao.AtualizaDados(fornecedor);

					var _fornecedor = dao.Consulta(id);
					var _quadro_societario = dao.QuadroSocietario(id);

					return new { fornecedor = _fornecedor, quadro_societario = _quadro_societario };
					// fornecedorDao.MarcaVisitado(fornecedor.CnpjCpf, UserName);
				}

				//return fornecedor;
			}

			return null;
		}
	}
}
