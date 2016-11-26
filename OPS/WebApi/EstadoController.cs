using OPS.Dao;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class EstadoController : ApiController
    {
		EstadoDao dao;

		public EstadoController()
		{
			dao = new EstadoDao();
		}

		[HttpGet]
		[ActionName("Get")]
        [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public dynamic Consultar()
		{
			return dao.Consultar();
		}
	}
}
