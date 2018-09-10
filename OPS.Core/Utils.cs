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
        public static string FormataValor(object value)
        {
            if (value != null && !Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
                try
                {
                    return Convert.ToDecimal(value).ToString("#,##0.00");
                }
                catch
                {
                    // ignored
                }
            return "0,00";
        }

        public static string FormataData(object value)
        {
            if (value != null && !Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
                try
                {
                    return Convert.ToDateTime(value).ToString("dd/MM/yyyy");
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
                    return Convert.ToDateTime(value).ToString("dd/MM/yyyy HH:mm");
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

        public static async Task SendMailAsync(MailAddress objEmailTo, string subject, string body)
        {
            var lstEmailTo = new MailAddressCollection() {objEmailTo};
            await SendMailAsync(lstEmailTo, subject, body);
        }

        public static async Task SendMailAsync(MailAddressCollection lstEmailTo, string subject, string body)
        {
            using (var objEmail = new MailMessage
            {
                IsBodyHtml = true,
                Subject = subject,
                Body = body,
                SubjectEncoding = Encoding.GetEncoding("ISO-8859-1"),
                BodyEncoding = Encoding.GetEncoding("ISO-8859-1"),
                From = new MailAddress("envio@ops.net.br", "[OPS] Operação Política Supervisionada")
            })
            {
                foreach (MailAddress objEmailTo in lstEmailTo)
                {
                    objEmail.To.Add(objEmailTo);
                }

                //ServicePointManager.ServerCertificateValidationCallback =
                //    (s, certificate, chain, sslPolicyErrors) => true;

                var objSmtp = new SmtpClient();
                await objSmtp.SendMailAsync(objEmail);
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

        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}