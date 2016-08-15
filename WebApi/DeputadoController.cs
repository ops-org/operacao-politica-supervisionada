using OPS.Core;
using OPS.Dao;
using System.Web.Http;

namespace OPS.WebApi
{
	public class DeputadoController : ApiController
	{
		DeputadoDao dao;

		public DeputadoController()
		{
			dao = new DeputadoDao();
		}

		[HttpGet]
		[ActionName("Get")]
		public dynamic Consultar(int id)
		{
			return dao.Consultar(id);
		}

		[HttpGet]
		public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		{
			return dao.Pesquisa(filtro);
		}

		[HttpGet]
		public dynamic Lancamentos([FromUri]FiltroParlamentarDTO filtro)
		{
			return dao.Lancamentos(filtro);
		}

		[HttpGet]
		public dynamic TipoDespesa([FromUri]FiltroDropDownDTO filtro)
		{
			return dao.TipoDespesa(filtro);
		}

		[HttpGet]
		public dynamic Secretarios([FromUri]FiltroParlamentarDTO filtro)
		{
			return dao.Secretarios(filtro);
		}

		[HttpGet]
		public dynamic SecretariosPorDeputado(int id)
		{
			return dao.SecretariosPorDeputado(id);
		}
	}
}
