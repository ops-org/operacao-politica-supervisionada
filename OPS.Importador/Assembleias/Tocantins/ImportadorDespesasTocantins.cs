using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using UglyToad.PdfPig;

namespace OPS.Importador.Assembleias.Tocantins
{
    public class ImportadorDespesasTocantins : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasTocantins(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.to.leg.br/transparencia/verbaIndenizatoria",
                Estado = Estados.Tocantins,
                ChaveImportacao = ChaveDespesaTemp.NomeCivil
            };
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {
            var document = await context.OpenAsyncAutoRetry(config.BaseAddress);
            var dcForm = new Dictionary<string, string>();
            dcForm.Add("transparencia.tipoTransparencia.codigo", "14");
            dcForm.Add("transparencia.ano", ano.ToString());
            dcForm.Add("transparencia.mes", mes.ToString());
            dcForm.Add("transparencia.parlamentar", "");
            IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form.py-4");
            document = await form.SubmitAsyncAutoRetry(dcForm);

            form = document.QuerySelector<IHtmlFormElement>("form.py-4");
            var gabinetes = (document.QuerySelector($"#transparencia_parlamentar") as IHtmlSelectElement);

            if (gabinetes.Options[2].Text == "Não existem registros para o ano selecionado")
            {
                logger.LogError("Não existem registros para o ano {Ano}.", mes);
                return;
            }

            foreach (var gabinete in gabinetes.Options)
            {
                if (gabinete.Value == "") continue;

                using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = gabinete.Text, ["Mes"] = mes }))
                {
                    dcForm = new Dictionary<string, string>();
                    dcForm.Add("transparencia.tipoTransparencia.codigo", "14");
                    dcForm.Add("transparencia.ano", ano.ToString());
                    dcForm.Add("transparencia.mes", mes.ToString());
                    dcForm.Add("transparencia.parlamentar", gabinete.Value);
                    var subDocument = await form.SubmitAsyncAutoRetry(dcForm);

                    //logger.LogInformation($"Consultando Parlamentar {gabinete.Value}: {gabinete.Text} para {mes:00}/{ano}");

                    var linksPdf = subDocument.QuerySelectorAll(".table-responsive-stack a");
                    if (linksPdf.Count() > 1)
                    {
                        logger.LogWarning("Dados não disponiveis para o parlamentar {Parlamentar} para {Mes:00}/{Ano}.", gabinete.Text, mes, ano);
                        continue;
                    }

                    if (linksPdf.Count() == 0)
                    {
                        var elError = subDocument.QuerySelector(".alert strong");
                        if (elError != null)
                        {
                            if (elError.TextContent == "Nenhum resultado encontrado!")
                            {
                                // Legislatura anterior
                                if (!(ano == 2023 && mes == 1))
                                    logger.LogInformation("Despesas do parlamentar {Parlamentar} indisponiveis para {Mes:00}/{Ano}. Detalhes: {Detalhes}", gabinete.Text, mes, ano, elError.TextContent);
                            }
                            else
                                logger.LogWarning("Erro ao consultar despesas do parlamentar {Parlamentar} para {Mes:00}/{Ano}. Detalhes: {Detalhes}", gabinete.Text, mes, ano, elError.TextContent);

                            continue;
                        }
                    }

                    var urlPdf = (linksPdf[0] as IHtmlAnchorElement).Href;

                    using (logger.BeginScope(new Dictionary<string, object> { ["Url"] = urlPdf, ["Arquivo"] = $"CLTO-{ano}-{mes}-{gabinete.Value}.pdf" }))
                    {
                        await ImportarDespesasArquivo(ano, mes, gabinete, urlPdf);
                    }
                }
            }
        }

        private async Task ImportarDespesasArquivo(int ano, int mes, IHtmlOptionElement gabinete, string urlPdf)
        {
            var filename = $"{tempFolder}/CLTO-{ano}-{mes}-{gabinete.Value}.pdf";
            await fileManager.BaixarArquivo(dbContext, urlPdf, filename, config.Estado);

            using (PdfDocument document = PdfDocument.Open(filename, new ParsingOptions() { ClipPaths = true }))
            {
                // detect canditate table zones
                SimpleNurminenDetectionAlgorithm detector = new SimpleNurminenDetectionAlgorithm();
                //IExtractionAlgorithm ea = new BasicExtractionAlgorithm();
                IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();

                decimal valorTotalDeputado = 0;
                decimal valorMesAnterior = 0;
                string nomeCicilParlamentar = "";
                var totalValidado = false;
                var despesasIncluidas = 0;
                for (var p = 1; p <= document.NumberOfPages; p++)
                {
                    PageArea page = ObjectExtractor.Extract(document, p);
                    var regions = detector.Detect(page);
                    IReadOnlyList<Table> tables = ea.Extract(page.GetArea(regions[0].BoundingBox)); // take first candidate area

                    //List<Table> tables = ea.Extract(page);
                    foreach (var row in tables[0].Rows)
                    {
                        var numColunas = row.Count;
                        var indice = numColunas == 8 ? 0 : 1;
                        var idxItem = indice++;
                        var idxTipo = indice++;
                        var idxNumero = indice++;
                        var idxData = indice++;
                        var idxEmitente = indice++;
                        var idxCnpj = indice++;
                        var idxValor = indice++;
                        var idxSaldo = indice++;

                        if (numColunas == 3)
                        {
                            idxTipo = 1;
                            idxNumero = 2;
                        }
                        else if (numColunas == 2)
                        {
                            idxTipo = 0;
                            idxNumero = 1;
                        }

                        if (row[idxItem].GetText().StartsWith("DEPUTAD"))
                        {
                            if (!string.IsNullOrEmpty(row[idxTipo].GetText()))
                                nomeCicilParlamentar = row[idxTipo].GetText().ToTitleCase().Trim();
                            else
                                nomeCicilParlamentar = row[idxItem].GetText().Split(":")[1].Split("Processo")[0].ToTitleCase().Trim();

                            continue;
                        }

                        var isRowTotalCol1 = row[idxItem].GetText().Equals("Total das despesas", StringComparison.OrdinalIgnoreCase) || row[idxItem].GetText().Equals("Gasto no mês", StringComparison.OrdinalIgnoreCase);
                        var isRowTotalCol2 = row[idxTipo].GetText().Equals("Total das despesas", StringComparison.OrdinalIgnoreCase) || row[idxTipo].GetText().Equals("Gasto no mês", StringComparison.OrdinalIgnoreCase);

                        if (isRowTotalCol1 || isRowTotalCol2)
                        {
                            totalValidado = true;
                            var valorTotalArquivo = Convert.ToDecimal(row[idxNumero - (isRowTotalCol2 ? 0 : 1)].GetText(), cultureInfo);
                            ValidaValorTotal(filename, valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

                            break;
                        }

                        // Resumo
                        //if(valorMesAnterior == 0 && row[1].GetText().StartsWith("Saldo do mês anterior", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    var valorTemp = row[2].GetText().Replace("-", "").Trim();
                        //    if (!string.IsNullOrEmpty(valorTemp))
                        //    {
                        //        valorMesAnterior = Convert.ToDecimal(valorTemp, cultureInfo);
                        //        valorTotalDeputado += valorMesAnterior;
                        //    }
                        //    continue;
                        //}

                        var nomeParlamentar = gabinete.Text.Trim().ToTitleCase();
                        if (row[idxTipo].GetText().Contains("telefone", StringComparison.OrdinalIgnoreCase) && row[idxNumero].GetText() != "-" && row[idxNumero].GetText() != "")
                        {
                            var despesaTemp1 = new CamaraEstadualDespesaTemp()
                            {
                                Nome = nomeParlamentar,
                                NomeCivil = nomeCicilParlamentar,
                                Ano = (short)ano,
                                Mes = (short)mes,
                                TipoDespesa = "Indenizações e Restituições",
                                DataEmissao = new DateOnly(ano, mes, 1),
                                Empresa = "Telefone",
                                Valor = Convert.ToDecimal(row[idxNumero].GetText(), cultureInfo),
                                Origem = filename
                            };

                            InserirDespesaTemp(despesaTemp1);
                            continue;
                        }

                        // Pagina 2 apenas com Resumo/Totalizadores
                        if (numColunas == 3 || numColunas == 2) continue;

                        #region Despesas fora do padrão
                        if (row[0].GetText() == "33" && row[2].GetText() == "6.476,19")
                        {
                            var despesaTemp1 = new CamaraEstadualDespesaTemp()
                            {
                                Nome = nomeParlamentar,
                                NomeCivil = nomeCicilParlamentar,
                                Ano = (short)ano,
                                Mes = (short)mes,
                                TipoDespesa = "Indenizações e Restituições",
                                Documento = "NDC-e/261305",
                                DataEmissao = new DateOnly(2023, 3, 8),
                                Empresa = "Auto Posto de Combustíveis Lago Sul Ltda",
                                CnpjCpf = Core.Utilities.Utils.RemoveCaracteresNaoNumericos("32.169.795/0001-52"),
                                Valor = Convert.ToDecimal("200,00", cultureInfo),
                                Origem = filename
                            };

                            //logger.LogInformation("Inserindo Item {Item} com valor: {Valor}!", row[idxItem].GetText(), despesaTemp1.Valor);
                            valorTotalDeputado += despesaTemp1.Valor;
                            despesasIncluidas++;

                            InserirDespesaTemp(despesaTemp1);
                            continue;
                        }
                        #endregion Despesas fora do padrão

                        if (!int.TryParse(row[idxItem].GetText(), out _))
                        {
                            if (idxItem == 1 && int.TryParse(row[0].GetText(), out _))
                            {
                                indice = 0;
                                idxItem = indice++;
                                idxTipo = indice++;
                                idxNumero = indice++;
                                idxData = indice++;
                                idxEmitente = indice++;
                                idxCnpj = indice++;
                                idxValor = indice++;
                                idxSaldo = indice++;
                            }
                            else if (!int.TryParse(row[idxNumero].GetText(), out _))
                                continue;
                        }

                        // Primeiro item (Opcional)
                        if (row[idxEmitente].GetText().StartsWith("Saldo de notas", StringComparison.OrdinalIgnoreCase))
                        {
                            var valorTemp = row[idxValor].GetText();
                            if (string.IsNullOrEmpty(valorTemp))
                                valorTemp = row[idxSaldo].GetText(); // Há um caso onde o valor está na coluna de Saldo.

                            valorMesAnterior = Convert.ToDecimal(valorTemp, cultureInfo);
                            if (valorMesAnterior > 40_000) valorMesAnterior = 0; // Se o valor é alto, é o saldo, que deve ser desconsiderado.

                            if (valorMesAnterior > 0)
                            {
                                valorTotalDeputado += valorMesAnterior;
                                logger.LogInformation("Ignorando item 'Saldo de notas do mês anterior': {ValorMesAnterior}!", valorMesAnterior);
                            }
                            continue;
                        }

                        CamaraEstadualDespesaTemp despesaTemp = null;
                        if (string.IsNullOrEmpty(row[idxSaldo].GetText()))
                        {
                            var cnpj = row[idxCnpj].GetText();
                            if (!string.IsNullOrEmpty(row[idxValor].GetText()) || cnpj.Length > 18)
                            {
                                // Caso onde não há nome do fornecedor, os indices recuam
                                // Mapear o nome da empresa para uma coluna vazia (saldo)
                                despesaTemp = new CamaraEstadualDespesaTemp()
                                {
                                    Nome = nomeParlamentar,
                                    NomeCivil = nomeCicilParlamentar,
                                    Ano = (short)ano,
                                    Mes = (short)mes,
                                    TipoDespesa = "Indenizações e Restituições",
                                    Documento = Core.Utilities.Utils.RemoveCaracteresNumericos(row[idxTipo].GetText().Trim()) + "/" + row[idxNumero].GetText().Trim(),
                                    DataEmissao = AjustaData(row[idxData].GetText(), ano, mes),
                                    Empresa = row[idxSaldo].GetText(),
                                    Origem = filename
                                };

                                if (cnpj.Length > 18)
                                {
                                    var cpfCnpj = cnpj.Substring(0, 18);
                                    var valor = cnpj.Substring(18);
                                    if (!cpfCnpj.Contains("/")) // é CPF?
                                    {
                                        cpfCnpj = cnpj.Substring(0, 14);
                                        valor = cnpj.Substring(14);
                                    }

                                    despesaTemp.Empresa = row[idxEmitente].GetText();
                                    despesaTemp.CnpjCpf = Core.Utilities.Utils.RemoveCaracteresNaoNumericos(cpfCnpj);
                                    despesaTemp.Valor = Convert.ToDecimal(valor, cultureInfo);
                                }
                                else
                                {
                                    despesaTemp.CnpjCpf = Core.Utilities.Utils.RemoveCaracteresNaoNumericos(row[idxCnpj - 1].GetText());
                                    despesaTemp.Valor = Convert.ToDecimal(row[idxValor - 1].GetText(), cultureInfo);
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(row[idxSaldo - 1].GetText().Replace("-", "")))
                                        Convert.ToDecimal(row[idxSaldo - 1].GetText().Replace("(", "").Replace(")", ""), cultureInfo);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Verificar mapeamento invalido!");
                                }
                            }
                            else
                                continue;
                        }
                        else
                        {
                            despesaTemp = new CamaraEstadualDespesaTemp()
                            {
                                Nome = nomeParlamentar,
                                NomeCivil = nomeCicilParlamentar,
                                Ano = (short)ano,
                                Mes = (short)mes,
                                TipoDespesa = "Indenizações e Restituições",
                                Documento = Core.Utilities.Utils.RemoveCaracteresNumericos(row[idxTipo].GetText().Trim()) + "/" + row[idxNumero].GetText().Trim(),
                                DataEmissao = AjustaData(row[idxData].GetText(), ano, mes),
                                Empresa = row[idxEmitente].GetText(),
                                CnpjCpf = Core.Utilities.Utils.RemoveCaracteresNaoNumericos(row[idxCnpj].GetText()),
                                Valor = Convert.ToDecimal(row[idxValor].GetText(), cultureInfo),
                                Origem = filename
                            };

                            try
                            {
                                if (row[idxSaldo].GetText() != "-")
                                    Convert.ToDecimal(row[idxSaldo].GetText().Replace("(", "").Replace(")", ""), cultureInfo);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Verificar mapeamento invalido!");
                            }
                        }

                        //logger.LogInformation($"Inserindo Item {row[idxItem].GetText()} com valor: {despesaTemp.Valor}!");
                        valorTotalDeputado += despesaTemp.Valor;
                        despesasIncluidas++;

                        InserirDespesaTemp(despesaTemp);
                    }
                }

                if (!totalValidado)
                    logger.LogError("Valor total {ValorTotal} não validado!", valorTotalDeputado);
            }

            //var pagina     = Importacao    s.ReadPdfFileByLocationStrategy(filename).ToArray();

            ////if (paginasPdf.Count() > 2) Console.WriteLine("teste");

            //try
            //{
            //    Console.WriteLine(paginasPdf[0].Split('\n')[1]);
            //    Console.WriteLine(paginasPdf[0].Split('\n')[2]);

            //    Console.WriteLine(paginasPdf[0].Split('\n')[3]);
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine(paginasPdf[1].Split('\n')[0]);
            //}

            //for (int p = 0; p < paginasPdf.Count(); p++)
            //{
            //    var textoPdf = paginasPdf[p];
            //    string[] lines = textoPdf.Split('\n');
            //    var nomeEmpresa = string.Empty;

            //    for (int l = 0; l < lines.Length; l++) // Ignorar as 3 linhas iniciais da 1ª pagina.
            //    {
            //        try
            //        {

            //            var line = lines[l].Trim().RemoveSpaces();
            //            if (line.StartsWith("Valor mensal CODAP:"))
            //            {
            //                if (valorTotal != Convert.ToDecimal(line.Split(" ")[1].Trim(), cultureInfo))
            //                    throw new BusinessException("Verificar Valor Total Divergente!");

            //                return;
            //            }

            //            if(line.StartsWith("Item Tipo Nº Data"))
            //            {
            //                nomeEmpresa = string.Empty;
            //                continue;
            //            }

            //            if (!int.TryParse(line.Split(" ")[0], out _))
            //            {
            //                nomeEmpresa = line;
            //                continue;
            //            }

            //            var despesaTemp = new CamaraEstadualDespesaTemp()
            //            {
            //                Nome = gabinete.Text.Trim().ToTitleCase(),
            //                Ano = (short)ano,
            //                Mes = (short)mes,
            //                TipoDespesa = "Indenizações e Restituições"
            //            };

            //            // Item Tipo Nº Data CNPJ/CPF Valor Saldo
            //            string pattern = @"(?<item>\d*) (?<tipo>.*) (?<num>\d*) (?<data>\d{2}\/\d{2}\/\d{4}) (?<emitente>.*) (?<cnpj>[\d\.\/-]*) (?<valor>[\d\.,]*) (?<saldo>[\d\.,]*)"; // Sem nome da empresa
            //            if (!string.IsNullOrEmpty(nomeEmpresa))
            //            {
            //                pattern = @"(?<item>\d*) (?<tipo>.*) (?<num>\d*) (?<data>\d{2}\/\d{2}\/\d{4}) (?<cnpj>[\d\.\/-]*) (?<valor>[\d\.,]*) (?<saldo>[\d\.,]*)"; // Com sem da empresa
            //                despesaTemp.Empresa = nomeEmpresa;
            //                nomeEmpresa = string.Empty;
            //            }

            //            line = line.Replace("14/014/2023", "14/01/2023");
            //            Match match = Regex.Matches(line, pattern)[0];

            //            despesaTemp.Documento = match.Groups["tipo"].Value.Trim() + "/" + match.Groups["num"].Value.Trim();
            //            despesaTemp.DataEmissao = Convert.ToDateTime(match.Groups["data"].Value, cultureInfo);

            //            if (string.IsNullOrEmpty(despesaTemp.Empresa))
            //                despesaTemp.Empresa = match.Groups["emitente"]?.Value;

            //            despesaTemp.CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos(match.Groups["cnpj"].Value);
            //            despesaTemp.Valor = Convert.ToDecimal(match.Groups["valor"].Value, cultureInfo);

            //            valorTotal += despesaTemp.Valor;

            //            InserirDespesaTemp(despesaTemp);

            //        }
            //        catch (Exception ex)
            //        {
            //            logger.LogError(lines[l].Trim());
            //            logger.LogError(ex.Message);
            //        }
            //    }
            //}
        }

        private DateOnly AjustaData(string data, int ano, int mes)
        {
            switch (data)
            {
                case "": return new DateOnly(ano, mes, 1);
                case "14/014/2023": data = "14/01/2023"; break;
                case "29/02/2023": data = "28/02/2023"; break;
                case "16/03/2023Petroshop - Comércio de Combustíveis Ltda - ME": data = "16/03/2023"; break;
                case "020/0,3/2023": data = "20/03/2023"; break;
                case "21/03/2023A G T Comunicações - Eireli - ME": data = "21/03/2023"; break;
                case "29/0/2023": data = "29/03/2023"; break;
                case "12/047/2023": data = "12/04/2023"; break;
                case "19/015/2023": data = "19/05/2023"; break;
                case "238/06/2023": data = "28/06/2023"; break;
                case "18/074/2023": data = "18/07/2023"; break;
                case "1708/2023": data = "17/08/2023"; break;
                case "23/058/2023": data = "23/08/2023"; break;
                case "10/058/2023": data = "10/08/2023"; break;
                case "18/085/2023": data = "18/08/2023"; break;
                case "01/069/2023": data = "01/09/2023"; break;
                case "10/019/2023": data = "10/09/2023"; break;
                case "24/012024": data = "24/01/2024"; break;
                case "05/047/2024": data = "05/04/2024"; break;
                case "25/006/2024": data = "25/06/2024"; break;
                case "2706/2024": data = "27/06/2024"; break;
                case "23/072024": data = "23/07/2024"; break;
                case "23/072025": data = "23/07/2024"; break;
                case "25/102024": data = "25/10/2024"; break;
                case "1404/2025": data = "14/04/2025"; break;
                case "02/06/20255": data = "02/06/2025"; break;
                case "0107/2025": data = "01/07/2025"; break;
                case "21/047/2025": data = "21/07/2025"; break;
                case "15/0//2025": data = "15/08/2025"; break;
                case "404/10/2025": data = "04/10/2025"; break;
            }

            try
            {
                return DateOnly.Parse(data, cultureInfo);
            }
            catch (Exception)
            {
                logger.LogError("Data invalida: {Data}", data);
                return new DateOnly(ano, mes, 1);
            }

        }
    }
}
