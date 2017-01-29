using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPS.Core;

namespace OPS.Controllers
{
	public class MainController : Controller
	{
		/// <summary>
		/// This maps to the Main/Index.cshtml file.  This file is the main view for the application.
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			ViewBag.Title = "OPS - Operação Política Supervisionada";

            ViewBag.DeputadoFederalUltimaAtualizacao = Padrao.DeputadoFederalUltimaAtualizacao.ToString("dd/MM/yyyy HH:mm");
            ViewBag.SenadorUltimaAtualizacao = Padrao.SenadorUltimaAtualizacao.ToString("dd/MM/yyyy HH:mm");

            return View();
		}
	}
}