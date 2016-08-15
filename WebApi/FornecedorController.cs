using OPS.Core;
using OPS.Dao;
using OPS.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace OPS.WebApi
{
    public class FornecedorController : ApiController
    {
		FornecedorDao dao;

		public FornecedorController()
		{
			dao = new FornecedorDao();
		}

		[HttpGet]
		[ActionName("Get")]
		public Fornecedor Consulta(string value)
		{
			return dao.Consulta(value);
		}

		[HttpGet]
		public dynamic Pesquisa([FromUri] FiltroDropDownDTO filtro)
		{
			return dao.Pesquisa(filtro);
		}

		[HttpGet]
		public List<FornecedorQuadroSocietario> QuadroSocietario(string value)
		{
			return dao.QuadroSocietario(value);
		}
	}
}
