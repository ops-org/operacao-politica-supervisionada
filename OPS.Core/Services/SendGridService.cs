using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using OPS.Core.DTOs;
using RestSharp;

namespace OPS.Core.Services;

public class SendGridService
{
    public async Task SendMailAsync(string apiKey, MailAddress objEmailTo, string subject, string body, MailAddress ReplyTo = null, bool htmlContent = true)
    {
        var lstEmailTo = new MailAddressCollection() { objEmailTo };
        await SendMailAsync(apiKey, lstEmailTo, subject, body, ReplyTo, htmlContent);
    }

    public async Task SendMailAsync(string apiKey, MailAddressCollection lstEmailTo, string subject, string body, MailAddress ReplyTo = null, bool htmlContent = true)
    {
        if (lstEmailTo == null) return;

        var lstTo = new List<SendGridMessageTo>();
        foreach (MailAddress objEmailTo in lstEmailTo)
        {
            lstTo.Add(new SendGridMessageTo()
            {
                email = objEmailTo.Address,
                name = objEmailTo.DisplayName
            });
        }

        var param = new SendGridMessage()
        {
            personalizations = new List<SendGridMessagePersonalization>{
                    new SendGridMessagePersonalization()
                    {
                        to = lstTo,
                        subject = subject
                    }
                },
            content = new List<SendGridMessageContent>(){
                    new SendGridMessageContent()
                    {
                        type = htmlContent ? "text/html" : "text/plain",
                        value = body
                    }
                },
            from = new SendGridMessageFrom()
            {
                email = "envio@ops.net.br",
                name = "[OPS] Operação Política Supervisionada"
            }
        };

        if (ReplyTo != null)
        {
            param.reply_to = new SendGridMessageReplyTo()
            {
                email = ReplyTo.Address,
                name = ReplyTo.DisplayName
            };
        }

        // TODO: Refactor to use HttpClient instead of RestSharp
        using var restClient = new RestClient("https://api.sendgrid.com/v3/mail/send");

        var request = new RestRequest();
        request.AddHeader("content-type", "application/json");
        request.AddHeader("authorization", "Bearer " + apiKey);
        request.AddParameter("application/json", JsonSerializer.Serialize(param), ParameterType.RequestBody);
        RestResponse response = await restClient.PostAsync(request);

        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            var responseBody = JsonSerializer.Deserialize<dynamic>(response.Content);

            throw new Exception(responseBody["errors"][0]["message"].ToString());
        }
    }
}
