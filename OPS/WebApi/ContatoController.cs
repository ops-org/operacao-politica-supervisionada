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
                From = new MailAddress("envio@ops.net.br", "[OPS] Operação Política Supervisionada")
            };

            objEmail.ReplyToList.Add(new MailAddress(jsonData["email"].ToString(), jsonData["name"].ToString()));

#if DEBUG
            objEmail.To.Add("vanderleidenir@hotmail.com");
#else
			objEmail.To.Add("luciobig@ops.net.br");
#endif

            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

            var objSmtp = new SmtpClient("smtp.umbler.com", 587)
            {
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mRemetente, mSenhaEmail)
            };

            objSmtp.Send(objEmail);
        }

        [HttpPost]
        public void SolicitarRestituicao(Newtonsoft.Json.Linq.JObject jsonData)
        {
            var mRemetente = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailComunicacao"];
            var mSenhaEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["SenhaEmailComunicacao"];

            string mensagem = @"
                    <p>Prezado deputado Beto Rosado.</p>
                    <p>Como cidadão brasileiro e colaborador da Operação Política Supervisionada (OPS), informo que o seu escritório político, sediado em Mossoró, abasteceu veículos no posto de combustíveis Laser. As despesas, que perfazem o valor de R$ 58.885,36, foram pagas pelo senhor e lhes foram reembolsadas pela Câmara dos Deputados com dinheiro da verba indenizatória.</p>
                    <p>Esta é a relação das notas fiscais emitidas pelo Posto Laser:</p>
                    <p>
                        <table style=""width: 250px;"">
                            <tr><th>Emissão</th><th>NF</th><th>Valor</th></tr>
                            <tr><td>06/06/2016</td><td>5040</td><td>R$ 3.981,42<td></tr>
                            <tr><td>09/05/2016</td><td>4930</td><td>R$ 3.733,97<td></tr>
                            <tr><td>08/04/2016</td><td>4813</td><td>R$ 3.822,50<td></tr>
                            <tr><td>14/03/2016</td><td>4735</td><td>R$ 3.980,88<td></tr>
                            <tr><td>15/02/2016</td><td>4663</td><td>R$ 3.502,00<td></tr>
                            <tr><td>22/12/2015</td><td>4496</td><td>R$ 3.621,78<td></tr>
                            <tr><td>25/11/2015</td><td>4386</td><td>R$ 3.350,75<td></tr>
                            <tr><td>27/10/2015</td><td>4282</td><td>R$ 3.830,41<td></tr>
                            <tr><td>28/09/2015</td><td>4153</td><td>R$ 3.708,13<td></tr>
                            <tr><td>26/08/2015</td><td>4040</td><td>R$ 3.122,06<td></tr>
                            <tr><td>24/07/2015</td><td>3920</td><td>R$ 3.764,15<td></tr>
                            <tr><td>29/06/2015</td><td>3757</td><td>R$ 3.629,50<td></tr>
                            <tr><td>29/05/2015</td><td>3580</td><td>R$ 3.570,35<td></tr>
                            <tr><td>28/04/2015</td><td>3442</td><td>R$ 3.728,53<td></tr>
                            <tr><td>27/03/2015</td><td>3335</td><td>R$ 3.620,02<td></tr>
                            <tr><td>26/02/2015</td><td>3214</td><td>R$ 3.918,91<td></tr>
                        </table>
                    </p>
                    <p>O posto é de propriedade de Carlos Jerônimo Dix-Sept Rosado Maia, seu tio – parente consanguíneo de terceiro grau, em linha colateral. De acordo com o Ato da Mesa 43/2009, em seu Art. 4º, § 13, não é permitido o uso da verba indenizatória para pagar despesas de bens fornecidos ou de serviços prestados por empresa ou entidade da qual o proprietário ou detentor de qualquer participação seja o deputado ou parente seu até o terceiro grau.</p>
                    <p>Diante deste fato, venho por intermédio deste e-mail solicitar ao senhor que restitua aos cofres públicos todo o valor acima descrito. Para isso, entre em contato com o Departamento Financeiro da Câmara dos Deputados (Defin), informe o ocorrido e solicite uma GRU – Guia de Recolhimento da União – no valor em questão e em seguida efetue o pagamento.</p>
                    <p>Peço ainda que publique em suas redes sociais e site este equívoco e também as providências tomadas.</p>
                    <p>O contribuinte brasileiro está cada vez mais insatisfeito com a classe política brasileira por razões públicas e notórias. Certo de poder contar com a sua lisura, com o respeito aos seus eleitores, com o cumprimento ao que determina o Decreto Presidencial Nº 7.203 e com o que dispõe o Art. 4º, §13, do Ato da Mesa 43/2009 da Câmara dos Deputados antecipadamente agradeço pela resolução deste problema.</p>
                    <p>Atenciosamente,<br />{0}<br />{1} - {2}</p>
                ";

            var objEmail = new MailMessage()
            {
                IsBodyHtml = true,
                Subject = "Solicitação de restituição de valor indevidamente reembolsado",
                Body = string.Format(mensagem, jsonData["nome"], jsonData["cidade"], jsonData["estado"]),
                SubjectEncoding = Encoding.GetEncoding("ISO-8859-1"),
                BodyEncoding = Encoding.GetEncoding("ISO-8859-1"),
                From = new MailAddress("envio@ops.net.br", "[OPS] Operação Política Supervisionada")
            };

            objEmail.CC.Add(jsonData["email"].ToString());
            objEmail.ReplyToList.Add(new MailAddress(jsonData["email"].ToString(), jsonData["nome"].ToString()));

            objEmail.To.Add("dep.betorosado@camara.gov.br");
            objEmail.Bcc.Add("luciobig@ops.net.br");

            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

            var objSmtp = new SmtpClient("smtp.umbler.com", 587)
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
