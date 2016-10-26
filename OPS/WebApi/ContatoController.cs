using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Http;

namespace OPS.WebApi
{
	public class ContatoController : ApiController
	{

		[HttpPost]
		public void Post(Newtonsoft.Json.Linq.JObject jsonData)
		{
			var mRemetente = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailComunicacao"];
			var mSenhaEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["SenhaEmailComunicacao"];

			var objEmail = new MailMessage()
			{
				IsBodyHtml = false,
				Subject = "[OPS] Contato - " + jsonData["name"].ToString(),
				Body = jsonData["comments"].ToString(),
				SubjectEncoding = Encoding.GetEncoding("ISO-8859-1"),
				BodyEncoding = Encoding.GetEncoding("ISO-8859-1"),
				From = new MailAddress(mRemetente, "OPS - Comunicação")
			};

			objEmail.ReplyToList.Add(new MailAddress(jsonData["email"].ToString(), jsonData["name"].ToString()));

#if DEBUG
			objEmail.CC.Add("vanderleidenir@hotmail.com");
#else
			objEmail.To.Add("lucio@ops.net.br");
			objEmail.CC.Add("suporte@ops.net.br");
#endif

			ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

			var objSmtp = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true,
				Timeout = 10000,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(mRemetente, mSenhaEmail)
			};

			objSmtp.Send(objEmail);
		}
	}
}
