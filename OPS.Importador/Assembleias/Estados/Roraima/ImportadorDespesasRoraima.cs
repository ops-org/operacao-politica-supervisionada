using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AngleSharp;
using DDDN.OdtToHtml;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Roraima
{
    public class ImportadorDespesasRoraima : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://transparencia.al.rr.leg.br/execucao-orcamentaria-e-financeira/verbas-indenizatorias-de-gabinete/",
                Estado = Estado.Roraima,
                ChaveImportacao = ChaveDespesaTemp.NomeCivil
            };
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano)
        {
            var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();

            var idElementoAno = document
                .QuerySelectorAll(".ui-tabs-nav li a")
                .FirstOrDefault(x => x.TextContent == ano.ToString());

            if (idElementoAno is null)
            {
                logger.LogWarning("Dados para o ano {Ano} ainda não disponiveis!", ano);
                return;
            }

            var meses = document.QuerySelector(idElementoAno.Attributes["href"].Value).QuerySelectorAll(".link-template-default .card-body");
            foreach (var item in meses)
            {
                var tipoAquivo = item.QuerySelector("img.wpdm_icon").Attributes["src"].Value;
                if (!tipoAquivo.EndsWith("doc.svg") && !tipoAquivo.EndsWith("docx.svg")) continue;

                var urlPdf = item.QuerySelector("a.wpdm-download-link").Attributes["data-downloadurl"].Value;
                if (urlPdf.Contains("-2/") && !tipoAquivo.EndsWith("docx.svg")) continue;
                var titulo = item.QuerySelector(".package-title a").TextContent.Replace("Dep.", "").Trim();
                if (titulo == "Renato de Souza Silva Junho - 2024")
                    titulo = "Renato de Souza Silva - Junho 2024";
                else if (titulo == "Renato de Souza Silva Julho - 2024")
                    titulo = "Renato de Souza Silva - Julho 2024";

                var tituloPartes = titulo.Split(new[] { '-', '–' });
                var nomeParlamentar = tituloPartes[0].Trim();

                var mes = ResolveMes(tituloPartes[1].Trim());

                using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes, ["Parlamentar"] = nomeParlamentar, ["Arquivo"] = $"CLRR-{ano}-{mes}-{nomeParlamentar}.odt" }))
                {
                    ImportarDespesasArquivo(ano, mes, nomeParlamentar, urlPdf);
                }
            }
        }

        private void ImportarDespesasArquivo(int ano, int mes, string nomeCivilParlamentar, string urlPdf)
        {
            var filename = $"{tempPath}/CLRR-{ano}-{mes}-{nomeCivilParlamentar}.odt";
            BaixarArquivo(urlPdf, filename);

            // structure will contain all the data returned form the ODT to HTML conversion
            OdtConvertedData convertedData = null;

            try
            {
                // open the ODT document on the file system and call the Convert method to convert the document to HTML
                using (IOdtFile odtFile = new OdtFile(filename))
                    convertedData = new OdtConvert().Convert(odtFile, new OdtConvertSettings());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao procesar arquivo de {Mes:00}/{Ano} do Parlamentar {Parlamentar}. Url: {UrlPdf}", mes, ano, nomeCivilParlamentar, urlPdf);

                File.Delete(filename);
                return;
            }

            var context = httpClientResilient.CreateAngleSharpContext();
            var document = context.OpenAsync(req => req.Content(convertedData.Html.ForceWindows1252ToUtf8Encoding())).GetAwaiter().GetResult();

            var linhas = document.QuerySelectorAll("table tr");

            decimal valorTotalDeputado = 0;
            int despesasIncluidas = 0;
            bool totalValidado = false;

            foreach (var row in linhas)
            {
                var colunas = row.QuerySelectorAll("td");
                if (colunas.Length == 0) continue;

                var coluna1 = colunas[0].TextContent.Replace("\u00A0", " ").Trim();

                if (coluna1.StartsWith("Parlamentar:"))
                {
                    var colunaParlametar = coluna1.Split(":")[1].Replace("\u00A0", " ").Trim();
                    if (!Utils.RemoveAccents(nomeCivilParlamentar).Equals(Utils.RemoveAccents(colunaParlametar), StringComparison.InvariantCultureIgnoreCase))
                    {
                        logger.LogWarning("Parlamentar divergente! Esperado: '{ParlamentarEsperado}'; Recebido: '{ParlamentarRecebido}'", nomeCivilParlamentar, colunaParlametar.ToTitleCase());
                        nomeCivilParlamentar = colunaParlametar.ToTitleCase();

                    }

                    if (!string.IsNullOrEmpty(colunas[1].TextContent)) // CLRR-2023-1-Meton Melo Maciel.odt
                    {
                        var colunaMes = colunas[1].TextContent.Split(":").Last().Replace("\u00A0", " ").Trim();
                        if (ResolveMes(colunaMes) != mes)
                            logger.LogError("Mês Divergente! Esperado: '{MesEsperado}'; Recebido: '{MesRecebido}'", mes, colunaMes);
                    }

                    continue;
                }

                if (coluna1.StartsWith("SOMA PARCIAL"))
                {
                    totalValidado = true;
                    var valorTemp = colunas[1].TextContent;
                    if (string.IsNullOrEmpty(valorTemp))
                        valorTemp = colunas[2].TextContent; // CLRR-2023-1-Meton Melo Maciel.odt

                    var valorTotalArquivo = Convert.ToDecimal(valorTemp.Replace("R$", "").Trim(), cultureInfo);
                    ValidaValorTotal(valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

                    break;
                }

                if (colunas.Length < 3) continue;
                //if (colunas.Length != 3)
                //{
                //    foreach (var coluna in row.QuerySelectorAll("td"))
                //    {
                //        Console.Write(coluna.TextContent.Trim());
                //        Console.Write(" | ");
                //    }

                //    Console.WriteLine();
                //}

                var item = colunas[0].TextContent.Trim();
                var tipoDespesa = colunas[1].TextContent.Trim().Replace("\u00A0", " ");
                var valor = colunas[2].TextContent.Trim();
                if (string.IsNullOrEmpty(valor))
                {
                    if (colunas.Length > 3)
                        valor = colunas[3].TextContent.Trim(); // CLRR-2023-10-Meton Melo Maciel.odt
                    else
                        continue;
                }

                if (item.Length != 3 || string.IsNullOrEmpty(valor) || valor == "VALOR R$" || valor == "c") continue;

                switch (item)
                {
                    case "6.1": // Geral (meios de comunicação, blog, influenciadores digitais e similares)
                        tipoDespesa = "Divulgação de Atividade Parlamentar";
                        break;
                    case "8.1": // Geral
                        tipoDespesa = "Pesquisas Sócio-Ecomômicas";
                        break;
                }

                var nomeCivilParlamentarEncoded = nomeCivilParlamentar.ForceWindows1252ToLatin1Encoding();
                CamaraEstadualDespesaTemp despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = nomeCivilParlamentarEncoded,
                    NomeCivil = nomeCivilParlamentarEncoded,
                    Ano = (short)ano,
                    Mes = (short)mes,
                    DataEmissao = new DateTime(ano, mes, 1),
                    TipoDespesa = tipoDespesa,
                    Valor = Convert.ToDecimal(valor.Replace("R$", "").Trim(), cultureInfo),
                };

                //logger.LogWarning($"Inserindo Item {tipoDespesa} com valor: {despesaTemp.Valor}!");

                InserirDespesaTemp(despesaTemp);
                valorTotalDeputado += despesaTemp.Valor;
                despesasIncluidas++;
            }

            if (!totalValidado)
            {
                logger.LogError("Valor Não Validado: {ValorTotalCalculado}; Referencia: {Mes}/{Ano}; Parlamentar: {Parlamentar}; Arquivo: {UrlArquivo}",
                                valorTotalDeputado, mes, ano, nomeCivilParlamentar, urlPdf);

                //foreach (var linha in linhas)
                //{
                //    foreach (var coluna in linha.QuerySelectorAll("td"))
                //    {
                //        Console.Write(coluna.TextContent.Trim());
                //        Console.Write(" | ");
                //    }

                //    Console.WriteLine();
                //}
            }
        }

        private int ResolveMes(string mes) => mes.Substring(0, 3).ToUpper() switch
        {
            "JAN" => 1,
            "FEV" => 2,
            "MAR" => 3,
            "ABR" => 4,
            "MAI" => 5,
            "JUN" => 6,
            "JUL" => 7,
            "AGO" => 8,
            "SET" => 9,
            "OUT" => 10,
            "NOV" => 11,
            "DEZ" => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(mes), $"Mês invalido: {mes}"),
        };
    }
}
