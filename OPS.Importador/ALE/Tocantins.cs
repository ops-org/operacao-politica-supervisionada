using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using UglyToad.PdfPig;
using static OPS.Importador.ALE.ImportadorDespesasPernambuco;

namespace OPS.Importador.ALE;

public class Tocantins : ImportadorBase
{
    public Tocantins(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarTocantins(serviceProvider);
        importadorDespesas = new ImportadorDespesasTocantins(serviceProvider);
    }
}

public class ImportadorDespesasTocantins : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasTocantins(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.al.to.leg.br/transparencia/verbaIndenizatoria",
            Estado = Estado.Tocantins,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();
        var gabinetes = (document.QuerySelector($"#transparencia_parlamentar") as IHtmlSelectElement);

        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form.py-4");

        foreach (var gabinete in gabinetes.Options)
        {
            if (gabinete.Value == "") continue;

            using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = gabinete.Text, ["Mes"] = mes }))
            {
                var dcForm = new Dictionary<string, string>();
                dcForm.Add("transparencia.ano", ano.ToString());
                dcForm.Add("transparencia.mes", mes.ToString());
                dcForm.Add("transparencia.parlamentar", gabinete.Value);
                var subDocument = form.SubmitAsyncAutoRetry(dcForm).GetAwaiter().GetResult();

                //logger.LogInformation($"Consultando Parlamentar {gabinete.Value}: {gabinete.Text} para {mes:00}/{ano}");

                var linksPdf = subDocument.QuerySelectorAll(".table-responsive-stack a");
                if (linksPdf.Count() > 1) throw new NotImplementedException();
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
                    ImportarDespesasArquivo(ano, mes, gabinete, urlPdf);
                }
            }
        }
    }

    private void ImportarDespesasArquivo(int ano, int mes, IHtmlOptionElement gabinete, string urlPdf)
    {
        var filename = $"{tempPath}/CLTO-{ano}-{mes}-{gabinete.Value}.pdf";
        BaixarArquivo(urlPdf, filename);

        using (PdfDocument document = PdfDocument.Open(filename, new ParsingOptions() { ClipPaths = true }))
        {
            ObjectExtractor oe = new ObjectExtractor(document);

            // detect canditate table zones
            SimpleNurminenDetectionAlgorithm detector = new SimpleNurminenDetectionAlgorithm();
            //IExtractionAlgorithm ea = new BasicExtractionAlgorithm();
            IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();

            decimal valorTotalDeputado = 0;
            string nomeParlamentar = "";
            var totalValidado = false;
            var despesasIncluidas = 0;
            for (var p = 1; p <= document.NumberOfPages; p++)
            {
                PageArea page = oe.Extract(p);
                var regions = detector.Detect(page);
                List<Table> tables = ea.Extract(page.GetArea(regions[0].BoundingBox)); // take first candidate area

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

                    if (row[idxItem].GetText().StartsWith("DEPUTAD"))
                    {
                        if (!string.IsNullOrEmpty(row[idxTipo].GetText()))
                            nomeParlamentar = row[idxTipo].GetText().ToTitleCase();
                        else
                            nomeParlamentar = row[idxItem].GetText().Split(":")[1].Split("Processo")[0].ToTitleCase();

                        continue;
                    }

                    if (row[idxTipo].GetText().Equals("Total das despesas", StringComparison.OrdinalIgnoreCase) || row[idxTipo].GetText().Equals("Gasto no mês", StringComparison.OrdinalIgnoreCase))
                    {
                        totalValidado = true;
                        var valorTotalArquivo = Convert.ToDecimal(row[idxNumero].GetText(), cultureInfo);
                        if (valorTotalDeputado != valorTotalArquivo)
                            logger.LogError("Valor Divergente! Esperado: {ValorTotalArquivo}; Encontrado: {ValorTotalDeputado}; Diferenca: {Diferenca}",
                                valorTotalArquivo, valorTotalDeputado, valorTotalArquivo - valorTotalDeputado);

                        break;
                    }

                    if (row[idxTipo].GetText().Contains("telefone", StringComparison.OrdinalIgnoreCase) && row[idxNumero].GetText() != "-" && row[idxNumero].GetText() != "")
                    {
                        var despesaTemp1 = new CamaraEstadualDespesaTemp()
                        {
                            Nome = gabinete.Text.Trim().ToTitleCase(),
                            Cpf = nomeParlamentar,
                            Ano = (short)ano,
                            Mes = (short)mes,
                            TipoDespesa = "Indenizações e Restituições",
                            DataEmissao = new DateTime(ano, mes, 1),
                            Empresa = "Telefone",
                            Valor = Convert.ToDecimal(row[idxNumero].GetText(), cultureInfo)
                        };

                        InserirDespesaTemp(despesaTemp1);
                        continue;
                    }

                    // Pagina 2 apenas com Resumo/Totalizadores
                    if (numColunas == 3) continue;

                    if (row[0].GetText() == "33" && row[2].GetText() == "6.476,19")
                    {
                        var despesaTemp1 = new CamaraEstadualDespesaTemp()
                        {
                            Nome = gabinete.Text.Trim().ToTitleCase(),
                            Cpf = nomeParlamentar,
                            Ano = (short)ano,
                            Mes = (short)mes,
                            TipoDespesa = "Indenizações e Restituições",
                            Documento = "NDC-e/261305",
                            DataEmissao = new DateTime(2023, 3, 8),
                            Empresa = "Auto Posto de Combustíveis Lago Sul Ltda",
                            CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos("32.169.795/0001-52"),
                            Valor = Convert.ToDecimal("200,00", cultureInfo)
                        };

                        //logger.LogWarning("Inserindo Item {Item} com valor: {Valor}!", row[idxItem].GetText(), despesaTemp1.Valor);
                        valorTotalDeputado += despesaTemp1.Valor;
                        despesasIncluidas++;

                        InserirDespesaTemp(despesaTemp1);
                        continue;
                    }

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

                    if (row[idxEmitente].GetText().StartsWith("Saldo de notas", StringComparison.OrdinalIgnoreCase))
                    {
                        var valorTemp = row[idxValor].GetText();
                        if (string.IsNullOrEmpty(valorTemp))
                            valorTemp = row[idxSaldo].GetText(); // Há um caso onde o valor está na coluna de Saldo.

                        var valorMesAnterior = Convert.ToDecimal(valorTemp, cultureInfo);
                        valorTotalDeputado += valorMesAnterior;
                        logger.LogInformation("Ignorando item 'Saldo de notas do mês anterior': {ValorMesAnterior}!", valorMesAnterior);
                        continue;
                    }

                    CamaraEstadualDespesaTemp despesaTemp = null;
                    if (string.IsNullOrEmpty(row[idxSaldo].GetText()))
                    {
                        if (!string.IsNullOrEmpty(row[idxValor].GetText()) || row[idxCnpj].GetText().Contains("13.716.765/0001-74344,12"))
                        {
                            // Caso onde não há nome do fornecedor, os indices recuam
                            // Mapear o nome da empresa para uma coluna vazia (saldo)
                            despesaTemp = new CamaraEstadualDespesaTemp()
                            {
                                Nome = gabinete.Text.Trim().ToTitleCase(),
                                Cpf = nomeParlamentar.Trim(),
                                Ano = (short)ano,
                                Mes = (short)mes,
                                TipoDespesa = "Indenizações e Restituições",
                                Documento = Core.Utils.RemoveCaracteresNumericos(row[idxTipo].GetText().Trim()) + "/" + row[idxNumero].GetText().Trim(),
                                DataEmissao = AjustaData(row[idxData].GetText(), ano, mes),
                                Empresa = row[idxSaldo].GetText(),

                            };

                            if (row[idxCnpj].GetText() == "16.846.429/0001-34100,00")
                            {
                                despesaTemp.CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos("16.846.429/0001-341");
                                despesaTemp.Valor = 100;
                            }
                            else if (row[idxCnpj].GetText() == "02.862.352/0002-62100,00")
                            {
                                despesaTemp.CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos("02.862.352/0002-621");
                                despesaTemp.Valor = 100;
                            }
                            else if (row[idxCnpj].GetText().Contains("13.716.765/0001-74344,12"))
                            {
                                despesaTemp.Empresa = "Petroshop Comércio de Combustíveis Ltda";
                                despesaTemp.CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos("13.716.765/0001-74");
                                despesaTemp.Valor = 344.12M;
                            }
                            else
                            {
                                despesaTemp.CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos(row[idxCnpj - 1].GetText());
                                despesaTemp.Valor = Convert.ToDecimal(row[idxValor - 1].GetText(), cultureInfo);
                            }

                            try
                            {
                                if (row[idxSaldo - 1].GetText() != "-")
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
                            Nome = gabinete.Text.Trim().ToTitleCase(),
                            Cpf = nomeParlamentar,
                            Ano = (short)ano,
                            Mes = (short)mes,
                            TipoDespesa = "Indenizações e Restituições",
                            Documento = Core.Utils.RemoveCaracteresNumericos(row[idxTipo].GetText().Trim()) + "/" + row[idxNumero].GetText().Trim(),
                            DataEmissao = AjustaData(row[idxData].GetText(), ano, mes),
                            Empresa = row[idxEmitente].GetText(),
                            CnpjCpf = Core.Utils.RemoveCaracteresNaoNumericos(row[idxCnpj].GetText()),
                            Valor = Convert.ToDecimal(row[idxValor].GetText(), cultureInfo),
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

                    //logger.LogWarning($"Inserindo Item {row[idxItem].GetText()} com valor: {despesaTemp.Valor}!");
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

    private DateTime AjustaData(string data, int ano, int mes)
    {
        switch (data)
        {
            case "": return new DateTime(ano, mes, 1);
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
        }

        try
        {
            return Convert.ToDateTime(data, cultureInfo);
        }
        catch (Exception)
        {
            logger.LogError("Data invalida: {Data}", data);
            return new DateTime(ano, mes, 1);
        }

    }
}

public class ImportadorParlamentarTocantins : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarTocantins(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.to.leg.br/perfil",
            SeletorListaParlamentares = "#list-parlamentares .quadro-deputado",
            Estado = Estado.Tocantins,
            ColetaDadosDoPerfil = false
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        var nomeparlamentar = document.QuerySelector("h3").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (document.QuerySelector("a.btn-perfil") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (document.QuerySelector("img.foto-deputado") as IHtmlImageElement)?.Source;
        deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());
        deputado.IdPartido = BuscarIdPartido(document.QuerySelector("h5").TextContent.Trim());

        deputado.Telefone = document.QuerySelectorAll("h6")[0].TextContent.Trim();
        deputado.Email = document.QuerySelectorAll("h6")[1].TextContent.Trim();

        ImportacaoUtils.MapearRedeSocial(deputado, document.QuerySelectorAll(".profile-social-container a"));

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        throw new NotImplementedException();
    }
}
