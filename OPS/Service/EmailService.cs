using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;

namespace OPS.Service
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var sLoginEmail = WebConfigurationManager.AppSettings["EmailComunicacao"];
            var sSenhaEmail = WebConfigurationManager.AppSettings["SenhaEmailComunicacao"];

            using (var objEmail = new MailMessage
            {
                IsBodyHtml = false,
                Subject = message.Subject,
                Body = message.Body, SubjectEncoding = 
                Encoding.GetEncoding("ISO-8859-1"),
                BodyEncoding = Encoding.GetEncoding("ISO-8859-1"),
                From = new MailAddress("envio@ops.net.br", "[OPS] Operação Política Supervisionada")
            })
            {
                objEmail.To.Add(new MailAddress(message.Destination, string.Empty));
            
                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                var objSmtp = new SmtpClient("smtp.umbler.com", 587)
                {
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(sLoginEmail, sSenhaEmail)
                };

                return objSmtp.SendMailAsync(objEmail);
            }
        }
    }
}