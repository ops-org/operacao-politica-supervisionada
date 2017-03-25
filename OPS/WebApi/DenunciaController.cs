using System.Security.Claims;
using System.Web.Http;
using OPS.Core;
using OPS.Dao;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
    public class DenunciaComentarioDTO
    {
        public string id_denuncia { get; set; }
        public string texto { get; set; }
        public string situacao { get; set; }
    }

	[RoutePrefix("Api/Denuncia")]
	public class DenunciaController : ApiController
    {
        private readonly DenunciaDao _dao;

        public DenunciaController()
        {
            _dao = new DenunciaDao();
        }

        [HttpGet]
		[ActionName("")]
		[Authorize]
        public dynamic Lista([FromUri] FiltroDenunciaDTO filtro)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var userId = identity.FindFirst("UserId").Value;

            return _dao.Consultar(filtro, userId);
        }

        [HttpGet]
		[ActionName("{id:int}")]
		[Authorize]
        public dynamic Detalhes(int id)
        {
            return _dao.Consultar(id);
        }

        [HttpPost]
		[ActionName("AdicionarComentario")]
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