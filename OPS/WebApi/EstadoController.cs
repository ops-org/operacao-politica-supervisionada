using OPS.Dao;
using System.Web.Http;

namespace OPS.WebApi
{
    public class EstadoController : ApiController
    {
		EstadoDao dao;

		public EstadoController()
		{
			dao = new EstadoDao();
		}

		[HttpGet]
		[ActionName("Get")]
		public dynamic Consultar()
		{
			return dao.Consultar();
		}
	}
}
