using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OPS
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "SPA",
				url: "app/{folder}/{page}",
				defaults: new { controller = "App", action = "Load", folder = "", page = "" }
			);

			routes.MapRoute(
				name: "Default",
				url: "{action}.html/{id}",
				defaults: new { controller = "Main", action = "Index", id = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "Application",
				url: "{*url}",
				defaults: new { controller = "Main", action = "Index" }
			);
		}
	}
}
