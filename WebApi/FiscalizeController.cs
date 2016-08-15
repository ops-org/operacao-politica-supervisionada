using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OPS.WebApi
{
    public class FiscalizeController : ApiController
    {
		private const string fiscalizeUrl = "http://104.131.229.175/fiscalize/pro/";

		[HttpGet]
		public string Get()
		{
			var client = new System.Net.WebClient();
			client.Encoding = System.Text.Encoding.UTF8;

			return client.DownloadString(fiscalizeUrl + "json_lista_fiscalizacao_cache.php");
		}

		[HttpGet]
		public string Get(int id)
		{
			var client = new System.Net.WebClient();
			client.Encoding = System.Text.Encoding.UTF8;

			return client.DownloadString(fiscalizeUrl + "json_nota_fiscal.php?notaFiscalId=" + id.ToString());
		}
	}
}
