using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

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
            HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
                    context.Exception.GetBaseException().Message, context.Exception.GetBaseException());

            Task.Run(async () =>
            {
                await Utils.SendMailAsync(new MailAddress("suporte@ops.net.br"), "OPS :: Informe de erro",
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
    }
}