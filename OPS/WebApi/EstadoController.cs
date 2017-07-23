using System.Web.Http;
using OPS.Core.DAO;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Partido")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class EstadoController : ApiController
    {
		EstadoDao dao;

		public EstadoController()
		{
			dao = new EstadoDao();
		}

		[HttpGet]
		[ActionName("")]
        [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public dynamic Consultar()
		{
			return dao.Consultar();
		}
	}
}
