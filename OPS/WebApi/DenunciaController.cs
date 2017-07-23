using System.Security.Claims;
using System.Web.Http;
using OPS.Core.DAO;
using OPS.Core.DTO;
using OPS.Core.Models;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Denuncia")]
	public class DenunciaController : ApiController
    {
        private readonly DenunciaDao _dao;

        public DenunciaController()
        {
            _dao = new DenunciaDao();
        }

		[HttpGet]
		[Route("")]
		[Authorize]
        public dynamic Lista([FromUri] FiltroDenunciaDTO filtro)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var userId = identity.FindFirst("UserId").Value;

            return _dao.Consultar(filtro, userId);
        }

		[HttpGet]
		[Route("{value}")]
		[Authorize]
		public DenunciaModel Detalhes(string value)
		{
			return _dao.Consultar(value);
		}

		[HttpPost]
		[Route("AdicionarComentario")]
		[Authorize]
        public void AdicionarComentario(DenunciaComentarioDTO value)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userFullNane = identity.FindFirst("FullNane").Value;
            var userId = identity.FindFirst("UserId").Value;

            _dao.AdicionarComentario(value, userId, userFullNane);
        }
    }
}