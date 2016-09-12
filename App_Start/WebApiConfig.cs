using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using System.Web.Http.Routing;

namespace OPS
{
	public static class WebApiConfig
	{
		public static string UrlPrefix { get { return "Api"; } }
		public static string UrlPrefixRelative { get { return "~/Api"; } }

		//http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api
		public static void Register(HttpConfiguration config)
		{
			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute("DefaultApiGet", UrlPrefix + "/{controller}", new { id = RouteParameter.Optional, action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
			config.Routes.MapHttpRoute("DefaultApiGetById", UrlPrefix + "/{controller}/{id}", new { action = "Get" }, new { id = @"\d+", httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
			config.Routes.MapHttpRoute("DefaultApiPost", UrlPrefix + "/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

			config.Routes.MapHttpRoute("DefaultApiWithId", UrlPrefix + "/{controller}/{id}", null, new { id = @"\d+" });
			config.Routes.MapHttpRoute("DefaultApiWithActionId", UrlPrefix + "/{controller}/{action}/{id}", new { id = RouteParameter.Optional }, new { id = @"\d+" });
			config.Routes.MapHttpRoute("DefaultApiWithAction", UrlPrefix + "/{controller}/{action}/{value}", new { value = RouteParameter.Optional });

			var json = config.Formatters.JsonFormatter;
			config.Formatters.Clear();
			config.Formatters.Add(json);
			json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
		}
	}
}
