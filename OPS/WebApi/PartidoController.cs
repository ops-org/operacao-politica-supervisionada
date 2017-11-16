using OPS.Core;
using System.Web.Http;
using OPS.Core.DAO;
using WebApi.OutputCache.V2;
using System.Threading.Tasks;

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
		public async Task<IHttpActionResult> Consultar()
        {
            var result = await dao.Consultar();

			return Ok(result);
        }
    }
}
