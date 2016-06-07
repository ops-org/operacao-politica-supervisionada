using OPS.Core;
using OPS.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OPS.Api
{
	public class DeputadoController : ApiController
	{
		DeputadoDao dao;

		public DeputadoController()
		{
			dao = new DeputadoDao();
		}

		[HttpGet]
		public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		{
			return dao.Pesquisa(filtro);
		}

		[HttpGet]
		public dynamic LancamentosPorParlamentar([FromUri]FiltroParlamentarDTO filtro)
		{
			return dao.LancamentosPorParlamentar(filtro);
		}
	}
}
