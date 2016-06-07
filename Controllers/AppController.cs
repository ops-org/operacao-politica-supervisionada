using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPS.Controllers
{
	/// <summary>
	/// Create an ActionResult and PartialView for each angular partial view you want to attatch to a route in the angular app.js file.
	/// </summary>
	public class AppController : Controller
	{
		public ActionResult Load(string folder, string page)
		{
			string partialUrl;
			if (string.IsNullOrEmpty(folder))
			{
				partialUrl = page;
			}
			else
			{
				partialUrl = folder + "/" + page;
			}

			ViewEngineResult result = ViewEngines.Engines.FindView(ControllerContext, partialUrl, null);
			if (result.View != null)
			{
				switch (partialUrl.ToLower())
				{
					case "deputado/lista":
						ViewBag.dtUltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
						break;
				}
				return PartialView(partialUrl);
			}
			else
			{
				return PartialView("erro/404");
			}
		}
	}
}