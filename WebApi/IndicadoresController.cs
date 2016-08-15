using Newtonsoft.Json;
using OPS.Dao;
using System.Web.Http;

namespace OPS.WebApi
{
	public class IndicadoresController : ApiController
	{
		[HttpGet]
		public string ParlamentarResumoGastos()
		{
			var dt = ComandoSqlDao.RecuperarCardsIndicadores();

			return JsonConvert.SerializeObject(dt, Formatting.None);
		}

		[HttpGet]
		public string ResumoAuditoria()
		{
			var dt = ComandoSqlDao.ExecutarConsultaSimples(ComandoSqlDao.eGrupoComandoSQL.ResumoAuditoria);

			return JsonConvert.SerializeObject(dt, Formatting.None);
		}
	}
}
