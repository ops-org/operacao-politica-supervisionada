//using System;
//using System.Web.Http.Controllers;
//using System.Web.Http.Filters;
//using Newtonsoft.Json;

//namespace OPS.Core
//{
//	public class LoggingFilterAttribute : ActionFilterAttribute
//	{
//		public override void OnActionExecuting(HttpActionContext filterContext)
//		{
//			var controller = filterContext.ControllerContext.ControllerDescriptor.ControllerType.Name;
//			var action = filterContext.ActionDescriptor.ActionName;
//			var method = filterContext.Request.Method.Method;
//			var query_string = filterContext.Request.RequestUri.PathAndQuery;
//			var ip = Utils.GetIPAddress();

//			string parameters = JsonConvert.SerializeObject(filterContext.ActionArguments, Formatting.None, new JsonSerializerSettings
//			{
//				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
//			});

//			using (var banco = new Banco())
//			{
//				banco.AddParameter("ip", ip);
//				banco.AddParameter("controller", controller);
//				banco.AddParameter("action", action);
//				banco.AddParameter("method", method);
//				banco.AddParameter("query_string", query_string);
//				banco.AddParameter("parameters", parameters);

//				banco.ExecuteNonQuery(@"INSERT INTO trace (ip, controller, action, method, query_string, parameters) VALUES (@ip, @controller, @action, @method, @query_string, @parameters);");
//			}
//		}
//	}
//}
