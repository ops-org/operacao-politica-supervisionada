using System.Web.Http;
using OPS.Core.DAO;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Indicadores")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class IndicadoresController : ApiController
	{
		[HttpGet]
		[Route("ParlamentarResumoGastos")]
		public dynamic ParlamentarResumoGastos()
		{
			return ComandoSqlDao.RecuperarCardsIndicadores();
		}
	}
}
