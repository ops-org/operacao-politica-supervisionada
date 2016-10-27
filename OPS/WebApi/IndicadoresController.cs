using Newtonsoft.Json;
using OPS.Dao;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
    [CacheOutput(ClientTimeSpan = 3600 /* 1h */, ServerTimeSpan = 21600 /* 6h */)]
    public class IndicadoresController : ApiController
	{
		[HttpGet]
		public dynamic ParlamentarResumoGastos()
		{
			return ComandoSqlDao.RecuperarCardsIndicadores();
		}
	}
}
