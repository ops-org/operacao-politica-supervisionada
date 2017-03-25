using Newtonsoft.Json;
using OPS.Dao;
using System.Web.Http;
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
