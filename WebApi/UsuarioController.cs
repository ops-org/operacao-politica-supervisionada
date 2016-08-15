using OPS.Core;
using OPS.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OPS.WebApi
{
	public class UsuarioController : ApiController
	{
		UsuarioDao dao;

		public UsuarioController()
		{
			dao = new UsuarioDao();
		}

		[HttpGet]
		public dynamic Pesquisa([FromUri]FiltroUsuarioDTO filtro)
		{
			return dao.Pesquisa(filtro);
		}
	}
}
