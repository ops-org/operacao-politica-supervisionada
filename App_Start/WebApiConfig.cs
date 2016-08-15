using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System.Web.Http.Routing;

namespace OPS
{
	public static class WebApiConfig
	{
		//http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services
			// Configure Web API to use only bearer token authentication.
			config.SuppressDefaultHostAuthentication();
			config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute("DefaultApiGet", "Api/{controller}", new { id = RouteParameter.Optional, action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
			config.Routes.MapHttpRoute("DefaultApiGetById", "Api/{controller}/{id}", new { action = "Get" }, new { id = @"\d+", httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
			config.Routes.MapHttpRoute("DefaultApiPost", "Api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

			config.Routes.MapHttpRoute("DefaultApiWithId", "Api/{controller}/{id}", null, new { id = @"\d+" });
			config.Routes.MapHttpRoute("DefaultApiWithActionId", "Api/{controller}/{action}/{id}", new { id = RouteParameter.Optional }, new { id = @"\d+" });
			config.Routes.MapHttpRoute("DefaultApiWithAction", "Api/{controller}/{action}/{value}", new { value = RouteParameter.Optional });

			var json = config.Formatters.JsonFormatter;
			config.Formatters.Clear();
			config.Formatters.Add(json);
			json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
		}
	}
}
