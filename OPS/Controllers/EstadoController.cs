using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Mvc;
using OPS.Core.DAO;

namespace OPS.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
	[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class EstadoController : Controller
    {
		EstadoDao dao;

		public EstadoController()
		{
			dao = new EstadoDao();
		}

		[HttpGet]
        [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public dynamic Consultar()
		{
			return dao.Consultar();
		}
	}
}
