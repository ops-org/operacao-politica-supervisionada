using Newtonsoft.Json;
using OPS.Dao;
using System.Web.Http;

namespace OPS.WebApi
{
	public class IndicadoresController : ApiController
	{
		[HttpGet]
		public dynamic ParlamentarResumoGastos()
		{
			return ComandoSqlDao.RecuperarCardsIndicadores();
		}

		[HttpGet]
		public string ResumoAuditoria()
		{
			//var dt = ComandoSqlDao.ExecutarConsultaSimples(ComandoSqlDao.eGrupoComandoSQL.ResumoAuditoria);

			//return JsonConvert.SerializeObject(dt, Formatting.None);

			return
				@"[
					{""Nome"":""Parlamentares denunciados ao MPF e/ou TCU"",""Resultado"":""194""},
					{""Nome"":""Valor recuperado"",""Resultado"":""R$ 5.500.000,00""},
					{""Nome"":""Valor em denuncias em andamento"",""Resultado"":""R$ 580.000,00""}
				]";
		}
	}
}
