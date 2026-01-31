// using System;
// using System.Globalization;
// using System.IO;
// using System.Net.Http;
// using System.Text;
// using System.Text.RegularExpressions;
// using CsvHelper;
// using Microsoft.Extensions.Configuration;
// using OPS.Core;
// using OPS.Core.Utilities;

// namespace OPS.Importador
// {
//     public static class Partidos
//     {
//         public static void ImportarHistorico(IConfiguration configuration)
//         {
//             var rootPath = configuration["AppSettings:SiteRootFolder"];

//             var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
//             var sb = new StringBuilder();
//             var file = rootPath + @"\temp\convertcsv.csv";

//             int indice = 0;
//             int Legenda = indice++;
//             int Imagem = indice++;
//             int Sigla = indice++;
//             int Nome = indice++;
//             int Sede = indice++;
//             int Fundacao = indice++;
//             int RegistroSolicitacao = indice++;
//             int RegistroProvisorio = indice++;
//             int RegistroDefinitivo = indice++;
//             int Extincao = indice++;
//             int Motivo = indice++;

//             using (var banco = new AppDb())
//             {
//                 using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
//                 {
//                     using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
//                     {
//                         //csv.Configuration.Delimiter = ",";

//                         using (HttpClient client = new())
//                         {
//                             client.DefaultRequestHeaders.UserAgent.ParseAdd(Utils.DefaultUserAgent);

//                             while (csv.Read())
//                             {
//                                 if (csv[Imagem] == "LOGO") continue;

//                                 if (csv[Imagem] != "")
//                                 {
//                                     try
//                                     {
//                                         MatchCollection m1 = Regex.Matches(csv[Imagem], @"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.Singleline);
//                                         if (m1.Count > 0)
//                                         {
//                                             var link = m1[0].Groups[1].Value;

//                                             var arquivo = rootPath + @"\wwwroot\partidos\" + csv[Sigla].ToLower() + ".png";
//                                             if (!File.Exists(arquivo))
//                                                 client.DownloadFile(link, arquivo).Wait();
//                                         }
//                                     }
//                                     catch (Exception ex)
//                                     {
//                                         Console.WriteLine(ex.Message);
//                                     }
//                                 }

//                                 banco.AddParameter("legenda", csv[Legenda] != "-" ? csv[Legenda] : null);
//                                 banco.AddParameter("sigla", csv[Sigla] != "??" ? csv[Sigla] : null);
//                                 banco.AddParameter("nome", csv[Nome]);
//                                 banco.AddParameter("sede", csv[Sede] != "??" ? csv[Sede] : null);
//                                 banco.AddParameter("fundacao", AjustarData(csv[Fundacao]));
//                                 banco.AddParameter("registro_solicitacao", AjustarData(csv[RegistroSolicitacao]));
//                                 banco.AddParameter("registro_provisorio", AjustarData(csv[RegistroProvisorio]));
//                                 banco.AddParameter("registro_definitivo", AjustarData(csv[RegistroDefinitivo]));
//                                 banco.AddParameter("extincao", AjustarData(csv[Extincao]));
//                                 banco.AddParameter("motivo", csv[Motivo]);

//                                 banco.ExecuteNonQuery(
//                                     @"INSERT INTO partido_historico (
//                                         legenda, sigla, nome, sede, fundacao, registro_solicitacao, registro_provisorio, registro_definitivo, extincao, motivo
//                                     ) VALUES (
//                                         @legenda, @sigla, @nome, @sede, @fundacao, @registro_solicitacao, @registro_provisorio, @registro_definitivo, @extincao, @motivo
//                                     )");
//                             }
//                         }
//                     }
//                 }
//             }
//         }

//         private static DateTime? AjustarData(string d)
//         {
//             if (!d.Contains("??/??/??") && d != "ATUAL" && d != "-")
//             {
//                 d = d.Replace("??", "01");
//                 if (d.Length == 10)
//                     return DateTime.Parse(d);
//                 else
//                     return DateTime.ParseExact(d, "dd/MM/yy", CultureInfo.InvariantCulture);
//             }

//             return null;
//         }
//     }
// }
