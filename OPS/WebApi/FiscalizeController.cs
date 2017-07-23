using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Partido")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class FiscalizeController : ApiController
	{
		private const string fiscalizeUrl = "http://104.131.229.175/fiscalize/pro/";

		[HttpGet]
		public dynamic Get()
		{
			var client = new RestClient(fiscalizeUrl);
			var request = new RestRequest("json_lista_fiscalizacao_cache.php", Method.GET);

			IRestResponse<List<NotaFiscalResumo>> response = client.Execute<List<NotaFiscalResumo>>(request);

			var lstNotaFiscal = response.Data.Select(o => new
			{
				NotaFiscalId = o.notaFiscalId,
				SomaSuspeitas = o.somaSuspeitas,
				SomaConfiaveis = o.somaConfiaveis,
				SomaFiscalizacoes = o.somaFiscalizacoes,
				RazaoConfiaveis = o.razaoConfiaveis,
				nome_parlamentar = o.parlamentar,
				Valor = o.valor
			});

			return lstNotaFiscal.ToList();
		}

		[HttpGet]
		public dynamic Get(int id)
		{
			var client = new RestClient(fiscalizeUrl);
			var request = new RestRequest("json_nota_fiscal.php?notaFiscalId={id}", Method.GET);
			request.AddUrlSegment("id", id.ToString());

			IRestResponse<NotaFiscal> response = client.Execute<NotaFiscal>(request);

			var o = response.Data;
			var oNotaFiscal = new
			{
				NotaFiscalId = o.notaFiscalId,
				nome_parlamentar = o.parlamentar,
				Cota = o.cota,
				Uf = o.uf,
				Partido = o.partido,
				DataEmissao = Core.Utils.FormataData(o.dataEmissao),
				DataInclusao = Core.Utils.FormataData(o.dataInclusao),
				Descricao = o.descricao,
				DescricaoSubCota = o.descricaoSubCota,
				Beneficiario = o.beneficiario,
				CpfCnpj = o.cpfCnpj,
				//Ano = o.ano,
				//Mes = o.mes,
				NumeroDocumento = o.numeroDocumento,
				Parcela = o.parcela,
				TipoDocumentoFiscal = o.tipoDocumentoFiscal,
				NomePassageiro = o.nomePassageiro,
				TrechoViagem = o.trechoViagem,
				Valor = o.valor,
				ValorLiquido = o.valorLiquido,
				ValorGlosa = o.valorGlosa,
				IdCadastro = o.ideCadastro
			};

			return oNotaFiscal;

			//var client = new System.Net.WebClient();
			//client.Encoding = System.Text.Encoding.UTF8;

			//return client.DownloadString(fiscalizeUrl + "json_nota_fiscal.php?notaFiscalId=" + id.ToString());
		}
	}

	public class NotaFiscalResumo
	{
		public int notaFiscalId { get; set; }
		public int somaSuspeitas { get; set; }
		public int somaConfiaveis { get; set; }
		public int somaFiscalizacoes { get; set; }
		public double razaoConfiaveis { get; set; }
		public string parlamentar { get; set; }
		public string valor { get; set; }
	}

	public class NotaFiscal
	{
		public int notaFiscalId { get; set; }
		public string parlamentar { get; set; }
		public string cota { get; set; }
		public string uf { get; set; }
		public string partido { get; set; }
		public string dataEmissao { get; set; }
		public string descricao { get; set; }
		public string descricaoSubCota { get; set; }
		public string beneficiario { get; set; }
		public string cpfCnpj { get; set; }
		public string ano { get; set; }
		public string mes { get; set; }
		public string numeroDocumento { get; set; }
		public string parcela { get; set; }
		public string tipoDocumentoFiscal { get; set; }
		public string nomePassageiro { get; set; }
		public string trechoViagem { get; set; }
		public string valor { get; set; }
		public string valorLiquido { get; set; }
		public string valorGlosa { get; set; }
		public string ideCadastro { get; set; }
		public string dataInclusao { get; set; }
	}
}
