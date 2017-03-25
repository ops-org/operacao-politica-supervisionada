using OPS.Core;
using OPS.Dao;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Senador")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class SenadorController : ApiController
    {
		SenadorDao dao;

		public SenadorController()
		{
			dao = new SenadorDao();
		}

		[HttpGet]
		[Route("{id:int}")]
		public dynamic Consultar(int id)
		{
			return dao.Consultar(id);
		}

		[HttpGet]
		[Route("")]
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
		[Route("{id:int}/GastosMensaisPorAno")]
		public dynamic GastosMensaisPorAno(int id)
		{
			return dao.GastosMensaisPorAno(id);
		}

		//[HttpGet]
		//[Route("Documento/{id:int}")]
		//public dynamic Documento(int id)
		//{
		//	return dao.Documento(id);
		//}

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
	}
}
