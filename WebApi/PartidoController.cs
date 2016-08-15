using OPS.Core;
using OPS.Dao;
using System.Web.Http;

namespace OPS.WebApi
{
	public class PartidoController : ApiController
    {
		PartidoDao dao;

		public PartidoController()
		{
			dao = new PartidoDao();
		}

		//[HttpGet]
		//public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		//{
		//	return dao.Pesquisa(filtro);
		//}
	}
}
