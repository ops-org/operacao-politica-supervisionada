using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OPS.Core
{
    public static class Utils
    {
        public static string FormataValor(object value)
        {
            if (!Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
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
            if (!Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
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
            if (!Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
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
            if (Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()) || (d.ToString() == "0000-00-00 00:00:00") ||
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
                objEmail.To.Add(objEmailTo);

                //ServicePointManager.ServerCertificateValidationCallback =
                //    (s, certificate, chain, sslPolicyErrors) => true;

                var objSmtp = new SmtpClient();
                await objSmtp.SendMailAsync(objEmail);
            }
        }
    }
}