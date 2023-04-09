using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core;
using System;
using System.Net.Mail;
using System.Text.Json;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContatoController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public ContatoController(IConfiguration configuration, IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        [HttpPost]
        [ActionName("")]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync(string jsonData)
        {
            if (jsonData == null) throw new ArgumentNullException(nameof(jsonData));

            var form = JsonDocument.Parse(jsonData).RootElement;

            var Subject = "[OPS] Contato - " + form.GetProperty("name");
            var Body = form.GetProperty("comments").ToString();
            var From = new MailAddress("envio@ops.net.br", "[OPS] Operação Política Supervisionada");
            var objEmailTo = new MailAddressCollection();
            var ReplyTo = new MailAddress(form.GetProperty("email").ToString(), form.GetProperty("name").ToString());

#if DEBUG
            objEmailTo.Add(new MailAddress("vanderlei@ops.net.br", "Vanderlei"));
#else
			objEmailTo.Add(new MailAddress("luciobig@ops.net.br", "Lúcio Big"));
#endif

            using (var objSmtp = new SmtpClient())
            {

                await Utils.SendMailAsync(Configuration["AppSettings:SendGridAPIKey"], objEmailTo, Subject, Body, ReplyTo);
            }

            return Ok();
        }
    }
}