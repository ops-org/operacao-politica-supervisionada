using OPS.Dao;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
    [CacheOutput(ClientTimeSpan = 3600 /* 1h */, ServerTimeSpan = 21600 /* 6h */)]
    public class EstadoController : ApiController
    {
		EstadoDao dao;

		public EstadoController()
		{
			dao = new EstadoDao();
		}

		[HttpGet]
		[ActionName("Get")]
		public dynamic Consultar()
		{
			return dao.Consultar();
		}
	}
}
