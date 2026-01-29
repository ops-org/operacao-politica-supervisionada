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
using OPS.Core.DTOs;
using RestSharp;

namespace OPS.Core.Utilities
{
    public static partial class Utils
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
            if (string.IsNullOrEmpty(value)) return null;
            if (value.Length == 14) return FormatCNPJ(value);
            if (value.Length == 11) return FormatCPF(value);
            if (value.Length == 6) return FormatCPFParcial(value);
            return value;
        }

        /// <summary>
        /// Formatar uma string CNPJ
        /// </summary>
        /// <param name="cnpj">string CNPJ sem formatacao</param>
        /// <returns>string CNPJ formatada</returns>
        /// <example>Recebe '99999999999999' Devolve '99.999.999/9999-99'</example>

        public static string FormatCNPJ(string cnpj)
        {
            if(string.IsNullOrEmpty(cnpj)) return null;

            try
            {
                // Anonimized data
                return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";

            }
            catch (Exception)
            {
                return cnpj;
            }
        }

        /// <summary>
        /// Formatar uma string CPF
        /// </summary>
        /// <param name="cpf">string CPF sem formatacao</param>
        /// <returns>string CPF formatada</returns>
        /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

        public static string FormatCPF(string cpf)
        {
            return FormatCPFParcial(cpf.Substring(3, 6));
        }

        /// <summary>
        /// Formatar uma string CPF
        /// </summary>
        /// <param name="cpf">string CPF sem formatacao</param>
        /// <returns>string CPF formatada</returns>
        /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

        public static string FormatCPFParcial(string cpf)
        {
            try
            {
                return Convert.ToUInt64(cpf).ToString(@"***\.000\.000\-**");
            }
            catch (Exception)
            {
                return cpf;
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

        public static string RemoveCaracteresNaoNumericosExetoAsterisco(string str)
        {
            var value = Regex.Replace(str ?? "", @"[^\d*]", "");

            // Correção para CPF com mascara de Rondonia
            if (value.StartsWith("***", StringComparison.InvariantCultureIgnoreCase) && value.EndsWith("***", StringComparison.InvariantCultureIgnoreCase) && value.Length == 12)
                return value.Substring(0, 11);

            return value;
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

        public static DateTime? ToDate(this string value)
        {
            if (!string.IsNullOrEmpty(value?.Trim()))
                return Convert.ToDateTime(value);

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

        private static DisplayAttribute DisplayCustomAtribute(this Enum enumValue)
        {
            try
            {
                return enumValue?.GetType()
                        ?.GetMember(enumValue.ToString())
                        ?.FirstOrDefault()
                        ?.GetCustomAttribute<DisplayAttribute>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DisplayName(this Enum enumValue)
        {
            try
            {
                return enumValue
                    .DisplayCustomAtribute()
                    .GetName();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DisplayShortName(this Enum enumValue)
        {
            try
            {
                return enumValue
                   .DisplayCustomAtribute()
                   .GetShortName();
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static List<int> ObterNumerosLegislatura(int ano, int? mes = null)
        {
            var legislaturas = new List<int>();

            // Se o mês foi especificado, retorna apenas a legislatura daquele mês
            if (mes.HasValue)
            {
                int mesConsiderado = mes.Value;

                // Janeiro pertence à legislatura anterior
                int anoEfetivo = mesConsiderado == 1 ? ano - 1 : ano;

                int? legislatura = CalcularLegislatura(anoEfetivo);
                if (legislatura.HasValue)
                {
                    legislaturas.Add(legislatura.Value);
                }
            }
            else
            {
                // Sem mês especificado: Janeiro pertence à legislatura anterior e Fev-Dez pertencem à legislatura do ano atual

                // Legislatura de janeiro (ano anterior)
                int? legislaturaJaneiro = CalcularLegislatura(ano - 1);
                if (legislaturaJaneiro.HasValue)
                {
                    legislaturas.Add(legislaturaJaneiro.Value);
                }

                // Legislatura de fev-dez (ano atual)
                int? legislaturaAno = CalcularLegislatura(ano);
                if (legislaturaAno.HasValue && legislaturaAno != legislaturaJaneiro)
                {
                    legislaturas.Add(legislaturaAno.Value);
                }
            }

            return legislaturas;
        }

        private static int? CalcularLegislatura(int anoEfetivo)
        {
            if (anoEfetivo >= 2027)
            {
                // Calcula quantos ciclos de 4 anos se passaram desde 2023
                int ciclos = (anoEfetivo - 2027) / 4;
                return 58 + ciclos;
            }
            else if (anoEfetivo >= 2023)
            {
                return 57;
            }
            else if (anoEfetivo >= 2019)
            {
                return 56;
            }
            else if (anoEfetivo >= 2015)
            {
                return 55;
            }
            else if (anoEfetivo >= 2011)
            {
                return 54;
            }
            else if (anoEfetivo >= 2007)
            {
                return 53;
            }

            return null;
        }

        public static string GetStateCode(string stateName)
        {
            var state = stateName.Replace("Importacao", "", StringComparison.InvariantCultureIgnoreCase);

            return state switch
            {
                "Acre" => "AC",
                "Alagoas" => "AL",
                "Amapa" => "AP",
                "Amazonas" => "AM",
                "Bahia" => "BA",
                "Ceara" => "CE",
                "DistritoFederal" => "DF",
                "EspiritoSanto" => "ES",
                "Goias" => "GO",
                "Maranhao" => "MA",
                "MatoGrosso" => "MT",
                "MatoGrossoDoSul" => "MS",
                "MinasGerais" => "MG",
                "Para" => "PA",
                "Paraiba" => "PB",
                "Parana" => "PR",
                "Pernambuco" => "PE",
                "Piaui" => "PI",
                "RioDeJaneiro" => "RJ",
                "RioGrandeDoNorte" => "RN",
                "RioGrandeDoSul" => "RS",
                "Rondonia" => "RO",
                "Roraima" => "RR",
                "SantaCatarina" => "SC",
                "SaoPaulo" => "SP",
                "Sergipe" => "SE",
                "Tocantins" => "TO",

                "Senado" => "SF",
                "Camara" => "CF",
                _ => state,
            };

            //if(ano != null)
            //    code += $" {ano.ToString().Substring(2, 2)}";

            //if (mes != null)
            //    code += $"-{mes.Value.ToString("D2")}";

            //return code;
        }

        public static bool ValidaCNPJ(string cnpj)
        {
            int[] mt1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mt2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma; int resto; string digito; string TempCNPJ;

            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14)
                return false;

            if (cnpj == "00000000000000" || cnpj == "11111111111111" ||
             cnpj == "22222222222222" || cnpj == "33333333333333" ||
             cnpj == "44444444444444" || cnpj == "55555555555555" ||
             cnpj == "66666666666666" || cnpj == "77777777777777" ||
             cnpj == "88888888888888" || cnpj == "99999999999999")
                return false;

            TempCNPJ = cnpj.Substring(0, 12);
            soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(TempCNPJ[i].ToString()) * mt1[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            TempCNPJ = TempCNPJ + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(TempCNPJ[i].ToString()) * mt2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            cpf = cpf.Trim().Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;

            for (int j = 0; j < 10; j++)
                if (j.ToString().PadLeft(11, char.Parse(j.ToString())) == cpf)
                    return false;

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        public static bool IsCnpj(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            cnpj = cnpj.Trim().Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
                return false;

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

    }
}