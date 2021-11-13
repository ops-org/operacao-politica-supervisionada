using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OPS.Core
{
    public static class Utils
    {
        public static string FormataValor(object value, int decimais = 2)
        {
            if (value != null && !Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
                try
                {
                    return Convert.ToDecimal(value).ToString("N" + decimais.ToString());
                }
                catch
                {
                    // ignored
                }
            return Convert.ToDecimal(0).ToString("N" + decimais.ToString());
        }

        public static string FormataData(object value)
        {
            if (value != null && !Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
                try
                {
                    return Convert.ToDateTime(value.ToString()).ToString("dd/MM/yyyy");
                }
                catch
                {
                    // ignored
                }
            return "";
        }

        public static string FormataDataHora(object value)
        {
            if (value != null && !Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
                try
                {
                    return Convert.ToDateTime(value.ToString()).ToString("dd/MM/yyyy HH:mm");
                }
                catch
                {
                    // ignored
                }
            return "";
        }

        public static object ParseDateTime(object d)
        {
            if (d != null && Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()) || (d.ToString() == "0000-00-00 00:00:00") ||
                d.ToString().StartsWith("*"))
                return DBNull.Value;

            try
            {
                return Convert.ToDateTime(d);
            }
            catch (Exception)
            {
                return DBNull.Value;
            }
        }

        public static string FormatCnpjCpf(string value)
        {
            if (value.Length == 14) return FormatCNPJ(value);
            if (value.Length == 11) return FormatCPF(value);
            return value;
        }

        /// <summary>
        /// Formatar uma string CNPJ
        /// </summary>
        /// <param name="CNPJ">string CNPJ sem formatacao</param>
        /// <returns>string CNPJ formatada</returns>
        /// <example>Recebe '99999999999999' Devolve '99.999.999/9999-99'</example>

        public static string FormatCNPJ(string CNPJ)
        {
            try
            {
                return Convert.ToUInt64(CNPJ).ToString(@"00\.000\.000\/0000\-00");
            }
            catch (Exception)
            {
                return CNPJ;
            }

        }

        /// <summary>
        /// Formatar uma string CPF
        /// </summary>
        /// <param name="CPF">string CPF sem formatacao</param>
        /// <returns>string CPF formatada</returns>
        /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

        public static string FormatCPF(string CPF)
        {
            try
            {
                return Convert.ToUInt64(CPF).ToString(@"000\.000\.000\-00");
            }
            catch (Exception)
            {
                return CPF;
            }
        }
        /// <summary>
        /// Retira a Formatacao de uma string CNPJ/CPF
        /// </summary>
        /// <param name="Codigo">string Codigo Formatada</param>
        /// <returns>string sem formatacao</returns>
        /// <example>Recebe '99.999.999/9999-99' Devolve '99999999999999'</example>

        public static string RemoveCaracteresNaoNumericos(string str)
        {
            return Regex.Replace(str, @"[^\d]", "");
        }

        public static string MySqlEscapeNumberToIn(string str)
        {
            return Regex.Replace(str, @"[^\d,]", "");
        }

        public static string MySqlEscape(string str)
        {
            return Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%]",
                delegate (Match match)
                {
                    string v = match.Value;
                    switch (v)
                    {
                        case "\x00":            // ASCII NUL (0x00) character
                            return "\\0";
                        case "\b":              // BACKSPACE character
                            return "\\b";
                        case "\n":              // NEWLINE (linefeed) character
                            return "\\n";
                        case "\r":              // CARRIAGE RETURN character
                            return "\\r";
                        case "\t":              // TAB
                            return "\\t";
                        case "\u001A":          // Ctrl-Z
                            return "\\Z";
                        default:
                            return "\\" + v;
                    }
                });
        }

        public static async Task SendMailAsync(string SendGridAPIKey, MailAddress objEmailTo, string subject, string body, MailAddress ReplyTo = null)
        {
            var lstEmailTo = new MailAddressCollection() { objEmailTo };
            await SendMailAsync(SendGridAPIKey, lstEmailTo, subject, body, ReplyTo);
        }

        public static async Task SendMailAsync(string APIKey, MailAddressCollection lstEmailTo, string subject, string body, MailAddress ReplyTo = null)
        {
            var lstTo = new List<To>();
            foreach (MailAddress objEmailTo in lstEmailTo)
            {
                lstTo.Add(new To()
                {
                    email = objEmailTo.Address,
                    name = objEmailTo.DisplayName
                });
            }

            var param = new SendGridMessage()
            {
                personalizations = new List<Personalization>{
                    new Personalization()
                    {
                        to = lstTo,
                        subject = subject
                    }
                },
                content = new List<Content>(){
                    new Content()
                    {
                        type = "text/html",
                        value = body
                    }
                },
                from = new From()
                {
                    email = "envio@ops.net.br",
                    name = "[OPS] Operação Política Supervisionada"
                }
            };

            if (ReplyTo != null)
            {
                param.reply_to = new ReplyTo()
                {
                    email = ReplyTo.Address,
                    name = ReplyTo.DisplayName
                };
            }

            var restClient = new RestClient("https://api.sendgrid.com/v3/mail/send");
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Host", "api.sendgrid.com");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + APIKey);
            request.AddParameter("application/json", JsonConvert.SerializeObject(param), ParameterType.RequestBody);
            IRestResponse response = await restClient.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                JObject responseBody = JObject.Parse(response.Content);

                throw new Exception(responseBody["errors"][0]["message"].ToString());
            }
        }

        public static string SingleSpacedTrim(String s)
        {
            return new Regex(@"\s{2,}").Replace(s, " ");
        }

        public static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        //public static string GetIPAddress()
        //{
        //    System.Web.HttpContext context = System.Web.HttpContext.Current;
        //    string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        //    if (!string.IsNullOrEmpty(ipAddress))
        //    {
        //        string[] addresses = ipAddress.Split(',');
        //        if (addresses.Length != 0)
        //        {
        //            return addresses[0];
        //        }
        //    }

        //    return context.Request.ServerVariables["REMOTE_ADDR"];
        //}



        protected class SendGridMessage
        {
            public List<Personalization> personalizations { get; set; }
            public List<Content> content { get; set; }
            public From from { get; set; }
            public ReplyTo reply_to { get; set; }
        }
        protected class To
        {
            public string email { get; set; }
            public string name { get; set; }
        }

        protected class Personalization
        {
            public List<To> to { get; set; }
            public string subject { get; set; }
        }

        protected class Content
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        protected class From
        {
            public string email { get; set; }
            public string name { get; set; }
        }

        protected class ReplyTo
        {
            public string email { get; set; }
            public string name { get; set; }
        }
    }
}