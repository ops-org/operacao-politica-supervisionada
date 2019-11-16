using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OPS.Core;
using System.Net.Mail;
using System.Text;

namespace OPS.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContatoController : Controller
    {
        private IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public ContatoController(IConfiguration configuration, IHostingEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        [HttpPost]
        [ActionName("")]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync(JObject jsonData)
        {
            var Subject = "[OPS] Contato - " + jsonData["name"];
            var Body = jsonData["comments"].ToString();
            var From = new MailAddress("envio@ops.net.br", "[OPS] Operação Política Supervisionada");
            var objEmailTo = new MailAddressCollection();
            var ReplyTo = new MailAddress(jsonData["email"].ToString(), jsonData["name"].ToString());

#if DEBUG
            objEmailTo.Add(new MailAddress("vanderleidenir@hotmail.com", "Vanderlei Denir"));
#else
			objEmailTo.Add(new MailAddress("luciobig@ops.net.br", "Lúcio Big"));
#endif

            var objSmtp = new SmtpClient();
            await Utils.SendMailAsync(Configuration, objEmailTo, Subject, Body, ReplyTo);

            return Ok();
        }
    }
}