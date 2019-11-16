//using System.Security.Claims;
//using System.Web.Http;
//using Microsoft.AspNetCore.Mvc;
//using OPS.Core.DAO;
//using OPS.Core.DTO;
//using OPS.Core.Models;

//namespace OPS.WebApi
//{
//	[Route("Api/Denuncia")]
//	public class DenunciaController : Controller
//    {
//        private readonly DenunciaDao _dao;

//        public DenunciaController()
//        {
//            _dao = new DenunciaDao();
//        }

//		[HttpGet]
//		[Route("")]
//		[Authorize]
//        public dynamic Lista( FiltroDenunciaDTO filtro)
//        {
//            var identity = (ClaimsIdentity) User.Identity;
//            var userId = identity.FindFirst("UserId").Value;

//            return _dao.Consultar(filtro, userId);
//        }

//		[HttpGet]
//		[Route("{value}")]
//		[Authorize]
//		public DenunciaModel Detalhes(string value)
//		{
//			return _dao.Consultar(value);
//		}

//		[HttpPost]
//		[Route("AdicionarComentario")]
//		[Authorize]
//        public void AdicionarComentario(DenunciaComentarioDTO value)
//        {
//            var identity = (ClaimsIdentity)User.Identity;
//            var userFullNane = identity.FindFirst("FullNane").Value;
//            var userId = identity.FindFirst("UserId").Value;

//            _dao.AdicionarComentario(value, userId, userFullNane);
//        }
//    }
//}