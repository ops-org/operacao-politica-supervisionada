using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoNorte
{
    public class ImportadorDespesasRioGrandeDoNorte : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        //private readonly List<DeputadoEstadual> deputados;

        public ImportadorDespesasRioGrandeDoNorte(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.rn.leg.br/portal/verbas",
                Estado = Estado.RioGrandeDoNorte,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };

            // TODO: Filtrar legislatura atual
            //deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {
            var today = DateTime.Today;
            var address = $"{config.BaseAddress}";
            var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
            var gabinetes = document.QuerySelectorAll("select[name=deputado_id] option").ToList();
            var meses = document.QuerySelectorAll("select[name=mes_id] option").ToList();

            foreach (var item in gabinetes)
            {
                var gabinete = item as IHtmlOptionElement;
                if (string.IsNullOrEmpty(gabinete.Value)) continue;

                var filename = $"{tempPath}/CLRN-{ano}-{mes}-{gabinete.Value}.pdf";
                if (!File.Exists(filename))
                {
                    //var deputado = deputados.Find(x => 
                    //    gabinete.Text.Equals( x.NomeImportacao, StringComparison.InvariantCultureIgnoreCase) ||
                    //    gabinete.Text.Equals(x.NomeParlamentar, StringComparison.InvariantCultureIgnoreCase)
                    //);
                    //if (deputado == null)
                    //{
                    //    logger.LogError($"Deputado {gabinete.Value}: {gabinete.Text} não existe ou não possui gabinete relacionado!");
                    //}

                    var mesExtenso = $"{new DateTime(ano, mes, 1).ToString("MMMM", cultureInfo).ToTitleCase()}/{ano}";
                    var dcForm = new Dictionary<string, string>();
                    try
                    {
                        var mesSelecionado = meses.FirstOrDefault(x => (x as IHtmlOptionElement).Text == mesExtenso);
                        if (mesSelecionado == null)
                        {
                            // Não gerar alerta para o mês atual e anterior
                            if (!(ano == today.Year && mes >= (today.Month - 1)))
                                logger.LogWarning("Pesquisa para {Parlamentar} no mês {Mes:00}/{Ano} não disponivel!", gabinete.Text, mes, ano);

                            continue;
                        }

                        dcForm.Add("mes_id", (mesSelecionado as IHtmlOptionElement).Value);
                    }
                    catch
                    {
                        // Não gerar alerta para o mês atual e anterior
                        if (!(ano == today.Year && mes >= (today.Month - 1)))
                            logger.LogWarning("Pesquisa para {Parlamentar} no mês {Mes:00}/{Ano} não disponivel!", gabinete.Text, mes, ano);

                        continue;
                    }

                    dcForm.Add("deputado_id", gabinete.Value);
                    IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form.form-default");
                    var subDocument = form.SubmitAsyncAutoRetry(dcForm).GetAwaiter().GetResult();

                    if (subDocument.QuerySelector(".m9 p")?.TextContent == "Não foram encontradas verbas para os filtros informados") continue;

                    var linkPdf = subDocument.QuerySelectorAll("a").FirstOrDefault(x => x.TextContent == "Visualizar notas");
                    var urlPdf = (linkPdf as IHtmlAnchorElement).Href;

                    ImportarDespesasArquivo(ano, mes, gabinete, urlPdf);
                }
                else
                {
                    ImportarDespesasArquivo(ano, mes, gabinete, "");
                }
            }
        }

        private void ImportarDespesasArquivo(int ano, int mes, IHtmlOptionElement gabinete, string urlPdf)
        {
            var filename = $"{tempPath}/CLRN-{ano}-{mes}-{gabinete.Value}.pdf";
            if (!File.Exists(filename))
                httpClientResilient.DownloadFile(urlPdf, filename).GetAwaiter().GetResult();

            var paginasPdf = ImportacaoUtils.ReadPdfFile(filename).ToArray();
            decimal valorTotal = 0;
            //if (paginasPdf.Count() > 2) Console.WriteLine("teste");

            //var pageLines = paginasPdf[0].Split('\n');
            //if (pageLines.Length >= 4)
            //    Console.WriteLine(pageLines[4]);
            //else
            //    Console.WriteLine(paginasPdf[1].Split('\n')[0]);

            var totalValidado = true;
            var paginaInicial = 0;
            if (paginasPdf.Length > 1 && paginasPdf.Last().StartsWith("GOVERNO DO ESTADO DO RIO GRANDE DO NORTE")) // Nesse caso a pagina 2 já inclui o conteúdo pra 1.
                paginaInicial = paginasPdf.Length - 1;

            for (int p = paginaInicial; p < paginasPdf.Count(); p++)
            {
                var textoPdf = paginasPdf[p];
                string[] lines = textoPdf.Split('\n');
                var linhaCompleta = string.Empty;

                for (int l = (p == 0 ? 6 : 0); l < lines.Length; l++) // Ignorar as 5 linhas iniciais da 1ª pagina.
                {
                    try
                    {

                        var line = lines[l];
                        if (line.StartsWith("DATA DA DESPESA"))
                        {
                            linhaCompleta = "";
                            continue;
                        }

                        linhaCompleta += " " + line;
                        if (!line.Contains("R$")) continue;
                        if (line.StartsWith("Total:"))
                        {
                            totalValidado = true;
                            if (valorTotal != Convert.ToDecimal(line.Split("R$")[1].Trim(), cultureInfo))
                                throw new BusinessException("Verificar Valor Total Divergente!");

                            return;
                        }

                        linhaCompleta = linhaCompleta.Trim();

                        // Incluir CNPJ em falta nas linhas invalidas
                        if (linhaCompleta == "31/05/2023 RECIBO - CASA DURVAL PAIVA R$ 250,00")
                            linhaCompleta = "31/05/2023 RECIBO 01.396.800/0001-36 - CASA DURVAL PAIVA R$ 250,00";
                        else if (linhaCompleta == "30/06/2023 RECIBO - ASS. COMUNITÁRIA DE COMUNICAÇÃO E CULTURA DE SÃO JOSÉ DE MIPIBU R$ 2.700,00")
                            linhaCompleta = "30/06/2023 RECIBO 02.895.731/0001-78 - ASS. COMUNITÁRIA DE COMUNICAÇÃO E CULTURA DE SÃO JOSÉ DE MIPIBU R$ 2.700,00";
                        else if (linhaCompleta == "18/10/2023 FATURA - NEOENERGIA R$ 78,06")
                            linhaCompleta = "18/10/2023 FATURA 08.324.196/0001-81 - NEOENERGIA R$ 78,06";
                        else if (linhaCompleta == "27/11/2023 RECIBO - ASSOCIAÇÃO COMUNITÁRIA DE COMUNICAÇÃO E CULTURA DE SÃO JOSÉ DE MIPIBU/RN R$ 2.700,00")
                            linhaCompleta = "27/11/2023 RECIBO 02.895.731/0001-78 - ASSOCIAÇÃO COMUNITÁRIA DE COMUNICAÇÃO E CULTURA DE SÃO JOSÉ DE MIPIBU/RN R$ 2.700,00";
                        else if (linhaCompleta == "27/08/2025 005607 - POSTOS PINHEIRO BORGES MONTE CASTELO R$ 200,00")
                            linhaCompleta = "27/08/2025 005607 53.930.207/0003-93 - POSTOS PINHEIRO BORGES MONTE CASTELO R$ 200,00";

                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = gabinete.Text.Trim().ToTitleCase(),
                            Cpf = gabinete.Value,
                            Ano = (short)ano,
                            Mes = (short)mes,
                            TipoDespesa = "Indenizações e Restituições"
                        };

                        string pattern = @"(\d{2}\/\d{2}\/\d{4}) (.*)(\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}) - (.*)R\$ (.*)"; // CNPJ
                        if (linhaCompleta.Contains("###"))
                            pattern = @"(\d{2}\/\d{2}\/\d{4}) (.*)\#\#\#(\d{3}\.\d{3}\.\d{3}-\d{2})\#\#\# (.*)R\$ (.*)"; //CPF

                        Match match = Regex.Matches(linhaCompleta, pattern)[0];
                        despesaTemp.DataEmissao = Convert.ToDateTime(match.Groups[1].Value, cultureInfo);
                        despesaTemp.Documento = match.Groups[2].Value.Trim();

                        if (despesaTemp.Documento.Length > 100 || ano != despesaTemp.DataEmissao.Year || mes != despesaTemp.DataEmissao.Month)
                        {
                            if (match.Groups[1].Value == "31/05/2013")
                                despesaTemp.DataEmissao = new DateTime(ano, mes, 31);
                            else if (match.Groups[1].Value == "27/02/2028")
                                despesaTemp.DataEmissao = new DateTime(ano, mes, 27);
                            else if (match.Groups[1].Value == "11/05/2023")
                                despesaTemp.DataEmissao = new DateTime(ano, mes, 11);
                            else
                            {
                            }
                        }

                        despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(match.Groups[3].Value);
                        despesaTemp.Empresa = match.Groups[4].Value;

                        despesaTemp.Valor = Convert.ToDecimal(match.Groups[5].Value, cultureInfo);

                        int numero;
                        if (int.TryParse(despesaTemp.Documento, out numero))
                            despesaTemp.Documento = numero.ToString();


                        valorTotal += despesaTemp.Valor;
                        linhaCompleta = string.Empty;

                        InserirDespesaTemp(despesaTemp);

                    }
                    catch (Exception ex)
                    {
                        logger.LogError(linhaCompleta);
                        logger.LogError(ex.Message);

                        linhaCompleta = string.Empty;
                    }
                }
            }

            if (!totalValidado)
                logger.LogError("Valor total não validado!");
        }
    }
}
