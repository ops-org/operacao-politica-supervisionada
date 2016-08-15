using OPS.Core;
using OPS.Dao;
using System.Web.Http;

namespace OPS.WebApi
{
    public class UfController : ApiController
    {
		UfDao dao;

		public UfController()
		{
			dao = new UfDao();
		}

		//[HttpGet]
		//public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		//{
		//	return dao.Pesquisa(filtro);
		//}
	}
}
