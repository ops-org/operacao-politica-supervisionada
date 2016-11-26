using OPS.Core;
using OPS.Dao;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class PartidoController : ApiController
    {
        PartidoDao dao;

        public PartidoController()
        {
            dao = new PartidoDao();
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
