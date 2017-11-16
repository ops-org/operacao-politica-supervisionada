using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading;

namespace OPS.Core
{
	public class ExceptionHandlingAttribute : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext context)
		{
			if (context.Exception is BusinessException || context.Exception is OperationCanceledException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent(context.Exception.Message),
					ReasonPhrase = "Exception"
				});
			}

#if DEBUG
			//Log Critical errors
			Debug.WriteLine(context.Exception);

			string sMessage = context.Exception.GetBaseException().Message;
#else
			string infoAdicional = JsonConvert.SerializeObject(context.Request,
				Formatting.Indented, new JsonSerializerSettings
				{
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					ContractResolver = new IgnoreErrorPropertiesResolver()
				});

			var exBase = context.Exception.GetBaseException();
			HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
				exBase.Message + Environment.NewLine + "<code>" + infoAdicional + "</code>",
				exBase.GetBaseException());

			Task.Run(async () =>
			{
				await Utils.SendMailAsync(new MailAddress(Padrao.EmailEnvioErros), "OPS :: " + exBase.Message,
					httpUnhandledException.GetHtmlErrorMessage());
			}).Wait();

			string sMessage = "Ocorreu um erro, tente novamente ou entre em contato com o administrador.";
#endif

			var ex = context.Exception;
			context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
			   new
			   {
				   ExceptionMessage = sMessage,
				   RealStatusCode = (int)(ex is NotImplementedException || ex is ArgumentNullException ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest)
			   },
			   new JsonMediaTypeFormatter());

			base.OnException(context);
		}

		public override Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
		{
			var task = base.OnExceptionAsync(context, cancellationToken);
			return task.ContinueWith((t) =>
			{
				if (context.Exception is BusinessException || context.Exception is OperationCanceledException)
				{
					throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
					{
						Content = new StringContent(context.Exception.Message),
						ReasonPhrase = "Exception"
					});
				}

#if DEBUG
				//Log Critical errors
				Debug.WriteLine(context.Exception);

				string sMessage = context.Exception.GetBaseException().Message;
#else
			string infoAdicional = JsonConvert.SerializeObject(context.Request,
				Formatting.Indented, new JsonSerializerSettings
				{
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					ContractResolver = new IgnoreErrorPropertiesResolver()
				});

			var exBase = context.Exception.GetBaseException();
			HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
				exBase.Message + Environment.NewLine + "<code>" + infoAdicional + "</code>",
				exBase.GetBaseException());

			Task.Run(async () =>
			{
				await Utils.SendMailAsync(new MailAddress(Padrao.EmailEnvioErros), "OPS :: " + exBase.Message,
					httpUnhandledException.GetHtmlErrorMessage());
			}).Wait();

			string sMessage = "Ocorreu um erro, tente novamente ou entre em contato com o administrador.";
#endif

				var ex = context.Exception;
				context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
				   new
				   {
					   ExceptionMessage = sMessage,
					   RealStatusCode = (int)(ex is NotImplementedException || ex is ArgumentNullException ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest)
				   },
				   new JsonMediaTypeFormatter());

				//base.OnException(context);
			});
		}
	}

	public class IgnoreErrorPropertiesResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if (new[]{
				"Cache",
				"Files",
				"InputStream",
				"Filter",
				"Length",
				"Position",
				"ReadTimeout",
				"WriteTimeout",
				"LastActivityDate",
				"LastUpdatedDate",
				"Session",
				"Properties"
			}.Contains(property.PropertyName))
			{
				property.Ignored = true;
			}
			return property;
		}
	}
}