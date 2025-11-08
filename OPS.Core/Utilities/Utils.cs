using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;

namespace OPS.Core.Utilities
{
    public static class Utils
    {
        public const string DefaultUserAgent = "Mozilla/5.0 (compatible; OPS_bot/1.0; +https://ops.org.br)";

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

        public static string NascimentoFormatado(object value)
        {
            if (value != null && !Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
                try
                {
                    var dataNascimento = Convert.ToDateTime(value.ToString());

                    var idade = DateTime.Today.Year - dataNascimento.Year;
                    if (DateTime.Today.DayOfYear < dataNascimento.DayOfYear)
                        idade--;

                    return $"{dataNascimento:dd/MM/yyyy} ({idade} anos)";
                }
                catch
                {
                    // ignored
                }
            return "";
        }
        public static object ParseDateTime(object d)
        {
            if (d is null) return DBNull.Value;
            if (d != null && Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()) || d.ToString() == "0000-00-00 00:00:00" ||
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
            if (value.Length == 6) return FormatCPFParcial(value);
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
            return FormatCPFParcial(CPF.Substring(4, 6));
        }

        /// <summary>
        /// Formatar uma string CPF
        /// </summary>
        /// <param name="CPF">string CPF sem formatacao</param>
        /// <returns>string CPF formatada</returns>
        /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

        public static string FormatCPFParcial(string CPF)
        {
            try
            {
                return Convert.ToUInt64(CPF).ToString(@"***\.000\.000\-**");
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
            return Regex.Replace(str ?? "", @"[^\d]", "");
        }

        public static string RemoveCaracteresNumericos(string str)
        {
            return Regex.Replace(str ?? "", @"[\d]", "");
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

        public static async Task SendMailAsync(string apiKey, MailAddress objEmailTo, string subject, string body, MailAddress ReplyTo = null, bool htmlContent = true)
        {
            var lstEmailTo = new MailAddressCollection() { objEmailTo };
            await SendMailAsync(apiKey, lstEmailTo, subject, body, ReplyTo, htmlContent);
        }

        public static async Task SendMailAsync(string apiKey, MailAddressCollection lstEmailTo, string subject, string body, MailAddress ReplyTo = null, bool htmlContent = true)
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
                        type = htmlContent ? "text/html" : "text/plain",
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
            //restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Noncompliant: trust all certificates

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

        public static string SingleSpacedTrim(string s)
        {
            return new Regex(@"\s{2,}").Replace(s, " ");
        }

        public static byte[] SHA1Hash(string input)
        {
            using var sha1 = SHA1.Create();
            return sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.ASCII.GetString(encodedDataAsBytes);
        }

        public static string ReadAllText(string file)
        {
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
                return textReader.ReadToEnd();
        }

        public static string ToTitleCase(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower()).Replace("De ", "de ").Replace("Da ", "da ").Replace("Do ", "do ");
        }

        public static string ReduceWhitespace(this string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        public static string RemoveAccents(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return string.Concat(
              text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            ).Normalize(NormalizationForm.FormC);
        }

        public static string ForceWindows1252ToUtf8Encoding(this string text)
        {
            //const int WindowsCodepage1252 = 1252;
            //byte[] bytes = Encoding.GetEncoding(WindowsCodepage1252).GetBytes(text);
            //return Encoding.Latin1.GetString(bytes);
            byte[] bytes = Encoding.Default.GetBytes(text);
            var utf8Text = Encoding.UTF8.GetString(bytes);

            return utf8Text; // utf8Text.Replace(0x61, 'ã').Replace("á", "á").Replace("ç", "ç").Replace("ú", "ú");
        }

        public static string ForceWindows1252ToLatin1Encoding(this string text)
        {
            const int WindowsCodepage1252 = 1252;
            byte[] bytes = Encoding.GetEncoding(WindowsCodepage1252).GetBytes(text);
            return Encoding.Latin1.GetString(bytes);
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

        public static string NullIfEmpty(this string value)
        {
            if (!string.IsNullOrEmpty(value?.Trim()))
                return value;

            return null;
        }

        public static T NullIfEmpty<T>(this T value) where T : class
        {
            if (!string.IsNullOrEmpty(value?.ToString().Trim()))
                return value;

            return null;
        }

        public static T? NullIf<T>(this T left, T right) where T : struct
        {
            return EqualityComparer<T>.Default.Equals(left, right) ? null : left;
        }

        public static string DisplayName(this Enum enumValue)
        {
            try
            {
                return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        .GetName();
            }
            catch (Exception)
            {
                return null;
            }
        }

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