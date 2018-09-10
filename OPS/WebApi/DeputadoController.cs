using System.Web.Http;
using OPS.Core.DAO;
using OPS.Core.DTO;
using WebApi.OutputCache.V2;
using System.Threading.Tasks;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Deputado")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
	public class DeputadoController : ApiController
	{
		DeputadoDao dao;

		public DeputadoController()
		{
			dao = new DeputadoDao();
		}

		[HttpGet]
		[Route("{id:int}")]
		public dynamic Consultar(int id)
		{
			return dao.Consultar(id);
		}

		[HttpPost]
		[Route("Lista")]
		[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
		public dynamic Lista(FiltroParlamentarDTO filtro)
		{
			return dao.Lista(filtro);
		}

		[HttpGet]
		[Route("Pesquisa")]
		[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
		public dynamic Pesquisa()
		{
			return dao.Pesquisa();
		}

		[HttpGet]
		[Route("Lancamentos")]
		public dynamic Lancamentos([FromUri]FiltroParlamentarDTO filtro)
		{
			return dao.Lancamentos(filtro);
		}

		[HttpGet]
		[Route("TipoDespesa")]
		[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
		public dynamic TipoDespesa()
		{
			return dao.TipoDespesa();
		}

		[HttpGet]
		[Route("Secretarios")]
		public dynamic Secretarios([FromUri]FiltroParlamentarDTO filtro)
		{
			return dao.Secretarios(filtro);
		}

		[HttpGet]
		[Route("{id:int}/Secretarios")]
		public dynamic SecretariosPorDeputado(int id)
		{
			return dao.SecretariosPorDeputado(id);
		}

		[HttpGet]
		[Route("{id:int}/GastosMensaisPorAno")]
		public dynamic GastosMensaisPorAno(int id)
		{
			return dao.GastosMensaisPorAno(id);
		}

		[HttpGet]
		[Route("Documento/{id:int}")]
		public async Task<dynamic> Documento(int id)
		{
			var result = await dao.Documento(id);

		    if (result != null)
		        return Ok(result);
		    else
		        return NotFound();
		}

		[HttpGet]
		[Route("DocumentosDoMesmoDia/{id:int}")]
		public async Task<dynamic> DocumentosDoMesmoDia(int id)
		{
			var result = await dao.DocumentosDoMesmoDia(id);

			return Ok(result);
		}

		[HttpGet]
		[Route("DocumentosDaSubcotaMes/{id:int}")]
		public async Task<dynamic> DocumentosDaSubcotaMes(int id)
		{
			var result = await dao.DocumentosDaSubcotaMes(id);

			return Ok(result);
		}

		[HttpGet]
		[Route("{id:int}/MaioresNotas")]
		public dynamic MaioresNotas(int id)
		{
			return dao.MaioresNotas(id);
		}

		[HttpGet]
		[Route("{id:int}/MaioresFornecedores")]
		public dynamic MaioresFornecedores(int id)
		{
			return dao.MaioresFornecedores(id);
		}

		[HttpGet]
		[Route("{id:int}/ResumoPresenca")]
		public dynamic ResumoPresenca(int id)
		{
			return dao.ResumoPresenca(id);
		}

		[HttpGet]
		[Route("CamaraResumoMensal")]
		public dynamic CamaraResumoMensal()
		{
			return dao.CamaraResumoMensal();
		}

		[HttpGet]
		[Route("CamaraResumoAnual")]
		public dynamic CamaraResumoAnual()
		{
			return dao.CamaraResumoAnual();
		}

		[HttpGet]
		[Route("Frequencia/{id:int}")]
		public dynamic Frequencia(int id)
		{
			return dao.Frequencia(id);
		}

		[HttpGet]
		[Route("Frequencia")]
		public dynamic Frequencia([FromUri]FiltroFrequenciaCamaraDTO filtro)
		{
			return dao.Frequencia(filtro);
		}
	}
}
