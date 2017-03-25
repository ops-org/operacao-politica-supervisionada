using OPS.Core;
using OPS.Dao;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Partido")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
	public class PartidoController : ApiController
    {
        PartidoDao dao;

        public PartidoController()
        {
            dao = new PartidoDao();
        }

        [HttpGet]
		[Route("")]
		[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
		public dynamic Consultar()
        {
            return dao.Consultar();
        }
    }
}
