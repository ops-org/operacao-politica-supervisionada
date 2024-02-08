using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using OfficeOpenXml;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Distrito Federal
/// https://www.cl.df.gov.br/web/guest
/// https://www.cl.df.gov.br/web/portal-transparencia/
/// https://dadosabertos.cl.df.gov.br/View/conjuntos.html
/// </summary>
public class DistritoFederal : ImportadorBase
{
    public DistritoFederal(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarDistritoFederal(serviceProvider);
        importadorDespesas = new ImportadorDespesasDistritoFederal(serviceProvider);
    }
}

public class ImportadorDespesasDistritoFederal : ImportadorDespesasArquivo
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasDistritoFederal(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://dadosabertos.cl.df.gov.br/",
            Estado = Estado.DistritoFederal,
            ChaveImportacao = ChaveDespesaTemp.Cpf
        };
    }

    /// <summary>
    /// Dados a partir de 2013
    /// https://dadosabertos.cl.df.gov.br/View/verbas.html
    /// </summary>
    /// <param name="ano"></param>
    /// <returns></returns>
    public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        Dictionary<string, string> arquivos = new();
        string urlOrigem, caminhoArquivo;

        if (ano >= 2020)
        {
            urlOrigem = $"{config.BaseAddress}View/opendata/verbas/{ano}_verba_indenizatoria.xlsx";
            caminhoArquivo = $"{tempPath}/CLDF-{ano}.xlsx";

            arquivos.Add(urlOrigem, caminhoArquivo);
        }
        else if (ano == 2019)
        {
            CultureInfo usEnglish = new CultureInfo("pt-BR");

            for (int mes = 1; mes <= 12; mes++)
            {
                string nomeMes = usEnglish.DateTimeFormat.MonthNames[mes - 1];

                // Janeiro e Fevereiro usa apenas o prefixo do mês
                if (ano == 2019 && mes <= 2)
                    nomeMes = nomeMes.Substring(0, 3);

                urlOrigem = $"{config.BaseAddress}View/opendata/verbas/{ano}_{nomeMes.ToUpper()}_verba%20indenizatoria.xlsx";
                caminhoArquivo = $"{tempPath}/CLDF-{ano}-{mes}.xlsx";

                arquivos.Add(urlOrigem, caminhoArquivo);
            }
        }
        else
        {
            if (ano == 2018)
                urlOrigem = $"{config.BaseAddress}View/opendata/verbas/{ano}%20completa%20verba_indenizatoria.csv";
            else // if (ano <= 2017)
                urlOrigem = $"{config.BaseAddress}View/opendata/verbas/verba_indenizatoria_{ano}.csv";

            caminhoArquivo = $"{tempPath}/CLDF-{ano}.csv";
            arquivos.Add(urlOrigem, caminhoArquivo);
        }

        return arquivos;
    }

    public override void ImportarDespesas(string caminhoArquivo, int ano)
    {
        if (caminhoArquivo.EndsWith(".xlsx"))
            CarregaDadosXlsx(caminhoArquivo, ano);
        else
            CarregaDadosCsv(caminhoArquivo, ano);
    }

    private void CarregaDadosCsv(string file, int ano)
    {
        string sResumoValores = string.Empty;

        int indice = 0;
        int GABINETE = indice++;
        int NOME_DEPUTADO = indice++;
        int CPF_DEPUTADO = indice++;
        int NOME_FORNECEDOR = indice++;
        int CNPJ_CPF_FORNECEDOR = indice++;
        int DATA = indice++;
        int DOCUMENTO = indice++;
        int VALOR = indice++;
        int CPF_FORNECEDOR = 0;
        int CLASSIFICACAO = 0;

        if (ano == 2018 || ano == 2019)
        {
            indice = 0;
            NOME_DEPUTADO = indice++;
            CPF_DEPUTADO = indice++;
            NOME_FORNECEDOR = indice++;
            CNPJ_CPF_FORNECEDOR = indice++;
            CPF_FORNECEDOR = indice++;
            DOCUMENTO = indice++;
            DATA = indice++;
            VALOR = indice++;
            CLASSIFICACAO = indice++;
        }

        using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
        {
            short count = 0;

            using (var csv = new CsvReader(reader, CultureInfo.CreateSpecificCulture("pt-BR")))
            {
                while (csv.Read())
                {
                    count++;

                    if (count == 1)
                    {
                        if (ano == 2013 || ano == 2014)
                        {
                            if (
                                csv[GABINETE] != "Gabinete" ||
                                csv[NOME_DEPUTADO] != "Nome" ||
                                csv[CPF_DEPUTADO] != "CPF" ||
                                csv[NOME_FORNECEDOR] != "EMPRESA (OU PROFISSIONAL)" ||
                                csv[CNPJ_CPF_FORNECEDOR] != "CNPJ(ouCPF)" ||
                                csv[DATA] != "Data de Emissão" ||
                                csv[DOCUMENTO] != "NºDocumento" ||
                                csv[VALOR] != "Valor"
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }
                        else if (ano == 2015)
                        {
                            if (
                                csv[GABINETE] != "GAB" ||
                                csv[NOME_DEPUTADO] != "DEPUTADO" ||
                                csv[CPF_DEPUTADO] != "CPF" ||
                                csv[NOME_FORNECEDOR] != "LOCAL" ||
                                csv[CNPJ_CPF_FORNECEDOR] != "CNPJ" ||
                                csv[DATA] != "DATA" ||
                                csv[DOCUMENTO] != "NUMERO" ||
                                csv[VALOR] != "VALOR"
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }
                        else if (ano == 2016 || ano == 2017)
                        {
                            if (
                                csv[GABINETE] != "Gabinete" ||
                                csv[NOME_DEPUTADO] != "Nome" ||
                                csv[CPF_DEPUTADO] != "CPF" ||
                                csv[NOME_FORNECEDOR].ToUpper() != "EMPRESA (OU PROFISSIONAL)" ||
                                csv[CNPJ_CPF_FORNECEDOR] != "CNPJ (ou CPF)" ||
                                csv[DATA] != "Data de Emissão" ||
                                csv[DOCUMENTO] != "Nº Documento" ||
                                csv[VALOR].Trim() != "Valor"
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }
                        else if (ano == 2018)
                        {
                            if (
                                csv[NOME_DEPUTADO] != "Nome do(a) Deputado(a)" ||
                                csv[CPF_DEPUTADO] != "CPF do(a) Deputado(a)" ||
                                csv[NOME_FORNECEDOR] != "Nome do Estabelecimento" ||
                                csv[CNPJ_CPF_FORNECEDOR] != "CNPJ" ||
                                csv[CPF_FORNECEDOR] != "CPF" ||
                                csv[DOCUMENTO] != "No.  do Recibo ou NF" ||
                                csv[DATA] != "Data do Recibo" ||
                                csv[VALOR] != "Valor" ||
                                csv[CLASSIFICACAO] != "Classificação"
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }
                        else if (ano == 2019)
                        {
                            if (
                                csv[NOME_DEPUTADO] != "Nome do(a) Deputado(a)" ||
                                csv[CPF_DEPUTADO] != "CPF do(a) Deputado(a)" ||
                                csv[NOME_FORNECEDOR] != "Nome do Estabelecimento" ||
                                csv[CNPJ_CPF_FORNECEDOR] != "CNPJ" ||
                                csv[CPF_FORNECEDOR] != "CPF" ||
                                csv[DOCUMENTO] != "N°  do Recibo ou Nota Fiscal" ||
                                csv[DATA] != "Data do Recibo/NF" ||
                                csv[VALOR] != "Valor" ||
                                csv[CLASSIFICACAO] != "Classificação"
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }

                        // Pular linha de titulo
                        continue;
                    }

                    if (string.IsNullOrEmpty(csv[NOME_DEPUTADO])) continue; //Linha vazia

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Ano = (short)ano,
                        Cpf = !string.IsNullOrEmpty(csv[CPF_DEPUTADO]) ? Utils.RemoveCaracteresNaoNumericos(csv[CPF_DEPUTADO]) : "",
                        Nome = csv[NOME_DEPUTADO].Replace("Deputado", "").Replace("Deputada", "").ToTitleCase(),
                        Empresa = csv[NOME_FORNECEDOR].Trim().Replace("NÃO INFORMADO", "").Replace("DOCUMENTO DANIFICADO", "").Replace("não consta documento", "").Trim(),
                        Documento = csv[DOCUMENTO],
                    };

                    if (!string.IsNullOrEmpty(csv[CNPJ_CPF_FORNECEDOR]))
                        despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(csv[CNPJ_CPF_FORNECEDOR]);
                    else if (ano >= 2018 && !string.IsNullOrEmpty(csv[CPF_FORNECEDOR]))
                        despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(csv[CPF_FORNECEDOR]);

                    DateTime data;
                    if (DateTime.TryParse(csv[DATA], out data))
                        despesaTemp.DataEmissao = data;
                    else
                        // Quando a data não estiver difinida colocamos no feriado;
                        despesaTemp.DataEmissao = new DateTime(ano, 1, 1);

                    string valor = csv[VALOR].Replace(" .", "").Replace(" ", "");

                    // Valor 1.500.00 é na verdade 1.500,00
                    Regex myRegex = new Regex(@"\.(\d\d$)", RegexOptions.Singleline);
                    if (myRegex.IsMatch(valor))
                        valor = myRegex.Replace(valor, @",$1");

                    try
                    {
                        despesaTemp.Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor) : 0;
                    }
                    catch (Exception)
                    {
                        if (valor.EndsWith("."))
                            valor = valor.Substring(0, valor.Length - 1).Trim();

                        valor = valor.Replace(" ", "");
                        despesaTemp.Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor) : 0;
                    }

                    if (ano >= 2018)
                        despesaTemp.TipoDespesa = csv[CLASSIFICACAO];

                    if (string.IsNullOrEmpty(despesaTemp.CnpjCpf))
                        despesaTemp.Observacao = csv[NOME_FORNECEDOR];
                    else if (!Regex.IsMatch(despesaTemp.CnpjCpf, @"\d"))
                        despesaTemp.Observacao = despesaTemp.CnpjCpf + " - " + csv[NOME_FORNECEDOR];

                    InserirDespesaTemp(despesaTemp);
                }
            }
        }
    }

    private void CarregaDadosXlsx(string file, int ano)
    {
        string sResumoValores = string.Empty;

        int indice = 1;
        int NOME_DEPUTADO = indice++;
        int CPF_DEPUTADO = indice++;
        int NOME_FORNECEDOR = indice++;
        int CNPJ_CPF_FORNECEDOR = indice++;
        int CPF_FORNECEDOR = indice++;
        int DOCUMENTO = indice++;
        int DATA = indice++;
        int VALOR = indice++;
        int CLASSIFICACAO = indice++;
        int OBSERVACAO = indice++;

        int count = 0;

        {
            using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
            using (var package = new ExcelPackage(reader.BaseStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                for (int i = 1; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (i == 1)
                    {

                        if (ano == 2019)
                        {
                            if (
                                worksheet.Cells[i, NOME_DEPUTADO].Value.ToString() != "Nome do(a) Deputado(a)" ||
                                worksheet.Cells[i, CPF_DEPUTADO].Value.ToString() != "CPF do(a) Deputado(a)" ||
                                worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString() != "Nome do Estabelecimento" ||
                                worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value.ToString() != "CNPJ" ||
                                worksheet.Cells[i, CPF_FORNECEDOR].Value.ToString() != "CPF" ||
                                worksheet.Cells[i, DOCUMENTO].Value.ToString() != "N°  do Recibo ou Nota Fiscal" ||
                                worksheet.Cells[i, DATA].Value.ToString() != "Data do Recibo/NF" ||
                                worksheet.Cells[i, VALOR].Value.ToString() != "Valor" ||
                                worksheet.Cells[i, CLASSIFICACAO].Value.ToString() != "Classificação"
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }
                        else if (ano >= 2020)
                        {
                            // Ignorando a validação de 2 colunas pq esta com erro de envoding no arquivo original.
                            if (
                                worksheet.Cells[i, NOME_DEPUTADO].Value.ToString() != "Nome do Deputado" ||
                                worksheet.Cells[i, CPF_DEPUTADO].Value.ToString() != "CPF do Deputado" ||
                                worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString() != "Nome do Estabelecimento" ||
                                worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value.ToString() != "CNPJ" ||
                                worksheet.Cells[i, CPF_FORNECEDOR].Value.ToString() != "CPF" ||
                                // (worksheet.Cells[i, DOCUMENTO].Value.ToString() != "Número do Recibo/NF") ||
                                worksheet.Cells[i, DATA].Value.ToString() != "Data do Recibo/NF" ||
                                worksheet.Cells[i, VALOR].Value.ToString() != "Valor (R$)"
                            // || (worksheet.Cells[i, CLASSIFICACAO].Value.ToString() != "Classificação")
                            )
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                            }
                        }

                        // Pular linha de titulo
                        continue;
                    }

                    if (string.IsNullOrEmpty((string)worksheet.Cells[i, NOME_DEPUTADO].Value)) continue; //Linha vazia

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Ano = (short)ano,
                        Cpf = !string.IsNullOrEmpty(worksheet.Cells[i, CPF_DEPUTADO].Value.ToString()) ? Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[i, CPF_DEPUTADO].Value.ToString()) : "",
                        Nome = worksheet.Cells[i, NOME_DEPUTADO].Value.ToString().Replace("Deputado", "").Replace("Deputada", "").Trim().ToTitleCase(),
                        Empresa = worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString().Trim().Replace("NÃO INFORMADO", "").Replace("DOCUMENTO DANIFICADO", "").Replace("não consta documento", "").Trim(),
                        Documento = worksheet.Cells[i, DOCUMENTO].Value.ToString(),
                    };

                    string cnpj_cpf = "";
                    if (!string.IsNullOrEmpty((string)worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value))
                    {
                        cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value.ToString());
                    }
                    else if (!string.IsNullOrEmpty((string)worksheet.Cells[i, CPF_FORNECEDOR].Value))
                    {
                        cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[i, CPF_FORNECEDOR].Value.ToString());
                    }

                    if (cnpj_cpf == "0030659700311234") cnpj_cpf = "00306597009834";
                    if (cnpj_cpf == "016152224000170") cnpj_cpf = "01615224000170";
                    if (cnpj_cpf == "01080639185" || cnpj_cpf == "01080639186") cnpj_cpf = "01080639179"; // Fábio Silveira Felix

                    despesaTemp.CnpjCpf = cnpj_cpf;

                    if (!string.IsNullOrEmpty(worksheet.Cells[i, DATA].Value?.ToString()))
                    {
                        if (worksheet.Cells[i, 7].Value is double)
                            despesaTemp.DataEmissao = DateTime.FromOADate((double)worksheet.Cells[i, DATA].Value);
                        else if (worksheet.Cells[i, 7].Value is DateTime)
                            despesaTemp.DataEmissao = (DateTime)worksheet.Cells[i, DATA].Value;
                        else if (worksheet.Cells[i, 7].Value.ToString().Contains(" de ")) // 04 de julho de 2023
                            despesaTemp.DataEmissao = Convert.ToDateTime( worksheet.Cells[i, DATA].Value, cultureInfo);
                        else
                        {
                            var data = worksheet.Cells[i, DATA].Value.ToString();
                            if (data.Split("/").Length != 3)
                            {
                                data = data.Replace("/", "");
                                data = $"{data.Substring(0, 2)}/{data.Substring(2, 2)}/{data.Substring(4, 4)}";
                            }

                            despesaTemp.DataEmissao = Convert.ToDateTime(data.Replace("31/05/22021", "31/05/2021"));
                        }

                    }
                    else
                    {
                        // Quando a data não estiver difinida colocamos no 1º do ano;
                        despesaTemp.DataEmissao = new DateTime(ano, 1, 1);
                    }

                    string valor = worksheet.Cells[i, VALOR].Value.ToString();
                    // Valor 1.500.00 é na verdade 1.500,00
                    Regex myRegex = new Regex(@"\.(\d\d$)", RegexOptions.Singleline);
                    if (myRegex.IsMatch(valor))
                        valor = myRegex.Replace(valor, @",$1");

                    try
                    {
                        despesaTemp.Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor) : 0;
                    }
                    catch (Exception)
                    {
                        if (valor.EndsWith("."))
                            valor = valor.Substring(0, valor.Length - 1).Trim();

                        valor = valor.Replace(" ", "").Replace("R$", "").Replace("R4", "");
                        despesaTemp.Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor) : 0;
                    }

                    despesaTemp.TipoDespesa = worksheet.Cells[i, CLASSIFICACAO].Value.ToString().Trim();

                    if (!string.IsNullOrEmpty(worksheet.Cells[i, OBSERVACAO].Value?.ToString()))
                        despesaTemp.Observacao = cnpj_cpf + " - " + worksheet.Cells[i, OBSERVACAO].Value.ToString();

                    InserirDespesaTemp(despesaTemp);

                    count++;
                }
            }
        }
    }

    public override void AjustarDados()
    {
        connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = NULL WHERE cnpj_cpf = '';
UPDATE ops_tmp.cl_despesa_temp SET observacao = NULL WHERE observacao = 'não consta documento';
UPDATE ops_tmp.cl_despesa_temp SET despesa_tipo = null WHERE despesa_tipo IN('', 'VII', 'VIII', 'IV');
");
    }
}

public class ImportadorParlamentarDistritoFederal : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarDistritoFederal(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        /// Atenção: Estruturas diferentes
        /// https://www.cl.df.gov.br/web/guest/dep-fora-exercicio
        /// https://www.cl.df.gov.br/web/guest/deputados-2023-2026
        /// https://www.cl.df.gov.br/web/guest/deputados-2019-2022
        /// https://www.cl.df.gov.br/web/guest/legislaturas-anteriores/-/asset_publisher/2jS3/content/-2015-2018-s-c3-a9tima-legislatura?_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId=10794633&_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_redirect=https%3A%2F%2Fwww.cl.df.gov.br%2Fweb%2Fguest%2Flegislaturas-anteriores%3Fp_p_id%3Dcom_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3%26p_p_lifecycle%3D0%26p_p_state%3Dnormal%26p_p_mode%3Dview%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_cur%3D0%26p_r_p_resetCur%3Dfalse%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId%3D10794633

        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.cl.df.gov.br/web/guest/deputados-2023-2026",
            SeletorListaParlamentares = ".deputados-interno div",
            Estado = Estado.DistritoFederal,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var nomeparlamentar = parlamentar.QuerySelector(".card-title").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
        var partido = parlamentar.QuerySelector(".card-text").TextContent.Trim();
        if (partido.Contains("("))
        {
            var arrPartidos = partido.Split(new[] { '(', ')' });
            partido = arrPartidos[arrPartidos[0].Length > arrPartidos[1].Length ? 1 : 0].Trim();
        }
        deputado.IdPartido = BuscarIdPartido(partido);

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector(".informacoes-pessoais img") as IHtmlImageElement)?.Source;

        var detalhes = subDocument.QuerySelectorAll(".informacoes-pessoais .row .col-md-9 p span");
        deputado.NomeCivil = detalhes[0].TextContent.Trim().ToTitleCase();
        try
        {
            deputado.Nascimento = DateOnly.Parse(detalhes[2].TextContent.Trim(), cultureInfo);
            deputado.Profissao = detalhes[3].TextContent.Trim().ToTitleCase();

            deputado.Naturalidade = detalhes[1].TextContent.Trim();
        }
        catch (Exception)
        {
            deputado.Nascimento = DateOnly.Parse(detalhes[1].TextContent.Trim(), cultureInfo);
            deputado.Profissao = detalhes[2].TextContent.Trim().ToTitleCase();
        }

        //var gabinete = detalhes[3].TextContent.Trim();

        var rodape = subDocument.QuerySelectorAll(".journal-content-article a b");
        if (rodape.Length == 0)
            rodape = subDocument.QuerySelectorAll(".journal-content-article a h5");

        deputado.Telefone = rodape[0].TextContent.Trim();
        deputado.Email = rodape[1].TextContent.Trim();
    }

    //        public override async void ImportarParlamentares()
    //        {
    //            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
    //            var config = Configuration.Default.WithDefaultLoader();
    //            var context = BrowsingContext.New(config);

    //            using (var db = new AppDb())
    //            {
    //                //var address = $"https://www.cl.df.gov.br/web/guest/deputados-2019-2022";
    //                var address = $"https://www.cl.df.gov.br/web/guest/legislaturas-anteriores/-/asset_publisher/2jS3/content/-2015-2018-s-c3-a9tima-legislatura?_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId=10794633&_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_redirect=https%3A%2F%2Fwww.cl.df.gov.br%2Fweb%2Fguest%2Flegislaturas-anteriores%3Fp_p_id%3Dcom_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3%26p_p_lifecycle%3D0%26p_p_state%3Dnormal%26p_p_mode%3Dview%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_cur%3D0%26p_r_p_resetCur%3Dfalse%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId%3D10794633";
    //                var document = await context.OpenAsync(address);
    //                if (document.StatusCode != HttpStatusCode.OK)
    //                {
    //                    Console.WriteLine($"{address} {document.StatusCode}");
    //                };

    //                var parlamentares = document.QuerySelectorAll(".journal-content-article table p");
    //                foreach (var parlamentar in parlamentares)
    //                {
    //                    if (parlamentar.ToHtml() == "<p>&nbsp;</p>") continue;

    //                    var urlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
    //                    var nome = parlamentar.QuerySelector("strong").TextContent.Trim();
    //                    var partido = parlamentar.QuerySelector("p>span").TextContent.Trim();
    //                    if (partido.Contains("("))
    //                        partido = partido.Split(new[] { '(', ')' })[1];

    //                    //Thread.Sleep(TimeSpan.FromSeconds(15));
    //                    //var subDocument = await context.OpenAsync(urlPerfil);
    //                    //if (document.StatusCode != HttpStatusCode.OK)
    //                    //{
    //                    //    Console.WriteLine($"{urlPerfil} {subDocument.StatusCode}");
    //                    //    continue;
    //                    //};

    //                    //var urlImagem = (subDocument.QuerySelector(".informacoes-pessoais img") as IHtmlImageElement)?.Source;

    //                    //var detalhes = subDocument.QuerySelectorAll(".informacoes-pessoais .row .col-md-9 p span");
    //                    //var nomeCivil = detalhes[0].TextContent.Trim();
    //                    //var naturalidade = detalhes[1].TextContent.Trim();
    //                    //var nascimento = Convert.ToDateTime(detalhes[2].TextContent.Trim(), cultureInfo);
    //                    //var profissao = detalhes[3].TextContent.Trim();
    //                    ////var gabinete = detalhes[3].TextContent.Trim();

    //                    //var rodape = subDocument.QuerySelectorAll(".journal-content-article a b");
    //                    //var telefone = rodape[0].TextContent.Trim();
    //                    //var email = rodape[1].TextContent.Trim();


    //                    db.AddParameter("@partido", partido);
    //                    //db.AddParameter("@nascimento", nascimento.ToString("yyyy-MM-dd"));
    //                    //db.AddParameter("@nomeCivil", nomeCivil);
    //                    //db.AddParameter("@email", email);
    //                    //db.AddParameter("@naturalidade", naturalidade);
    //                    //db.AddParameter("@escolaridade", profissao);
    //                    //db.AddParameter("@telefone", telefone);
    //                    db.AddParameter("@nome", nome);
    //                    db.ExecuteNonQuery(@"
    //update cl_deputado set 
    //    id_partido = (SELECT id FROM partido where sigla like @partido OR nome like @partido) -- ,
    //    -- nascimento = @nascimento, 
    //    -- nome_civil = @nomeCivil, 
    //    -- email = @email, 
    //    -- naturalidade = @naturalidade,
    //    -- escolaridade = @profissao,
    //    -- telefone = @telefone
    //where id_estado = 53
    //and nome_parlamentar = @nome");

    //                    if (db.RowsAffected != 1)
    //                        Console.WriteLine(nome);

    //                }
    //            }
    //        }
}