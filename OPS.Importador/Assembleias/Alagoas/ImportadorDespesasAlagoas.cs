using System.Globalization;
using System.Text;
using AngleSharp;
using Dapper;
using iTextSharp.text.pdf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;
using RestSharp;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Graphics;
using File = System.IO.File;
using PdfDocument = UglyToad.PdfPig.PdfDocument;

namespace OPS.Importador.Assembleias.Alagoas
{
    public class ImportadorDespesasAlagoas : ImportadorDespesasRestApiAnual
    {
        public ComputerVisionOcr ocrService;

        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasAlagoas(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.al.leg.br/transparencia/orcamento-e-financas/viap-verba-indenizatoria-de-atividade-parlamentar",
                Estado = Estados.Alagoas,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };

            AppSettings appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            ocrService = new ComputerVisionOcr(appSettings);
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var document = await context.OpenAsyncAutoRetry($"{config.BaseAddress}/{ano}");

            var parlamentares = document.QuerySelectorAll("#content-core .tileHeadline a");
            foreach (var parlamentar in parlamentares)
            {
                var nomeParlamentar = parlamentar.TextContent;
                var urlParlamentarMeses = parlamentar.Attributes["href"].Value;

                using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = nomeParlamentar }))
                {
                    var subdocument = await context.OpenAsyncAutoRetry(urlParlamentarMeses);
                    var meses = subdocument.QuerySelectorAll("#content-core .tileHeadline a");

                    foreach (var mes in meses)
                    {
                        var mesExtenso = mes.TextContent;
                        var competencia = new DateOnly(ano, ResolveMes(mesExtenso), 1);
                        if (competencia.AddMonths(1) > today) continue;

                        using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = competencia.Month }))
                        {
                            var urlParlamentarMesDocumento = mes.Attributes["href"].Value;
                            var subsubdocument = await context.OpenAsyncAutoRetry(urlParlamentarMesDocumento);

                            var urlPdf = subsubdocument.QuerySelector("#content-core a").Attributes["href"].Value;
                            if (string.IsNullOrEmpty(urlPdf))
                            {
                                logger.LogWarning("Despesas indisponiveis para {Mes:00}/{Ano}.", competencia.Month, competencia.Year);
                                continue;
                            }

                            using (logger.BeginScope(new Dictionary<string, object> { ["Url"] = urlPdf, ["Arquivo"] = $"CLAL-{ano}-{competencia.Month}-{nomeParlamentar}.pdf" }))
                            {
                               await ImportarDespesasArquivo(competencia.Year, competencia.Month, urlPdf, nomeParlamentar, competencia);
                            }
                        }
                    }
                }
            }

            //TODO
            //ImportarEmpenhosParaComparacao(context, ano);
        }

        public async Task ImportarDespesasArquivo(int ano, int mes, string urlPdf, string nomeParlamentar, DateOnly competencia)
        {
            var fileName = Path.Combine(tempFolder, $"CLAL-{ano}-{mes}-{nomeParlamentar}.pdf");
           await fileManager.BaixarArquivo(dbContext, urlPdf, fileName, config.Estado);

            var ocrFileName = fileName.Replace(".pdf", ".txt");
            if (!File.Exists(ocrFileName))
            {
                using (PdfDocument document = PdfDocument.Open(fileName))
                {
                    var imageIndex = 0;
                    var pages = document.GetPages();
                    if (pages.Count() > 1)
                    {

                        var sb = new StringBuilder();
                        foreach (Page page in pages)
                        {
                            IReadOnlyList<Letter> letters = page.Letters;
                            string example = string.Join(string.Empty, letters.Select(x => x.Value));

                            IEnumerable<UglyToad.PdfPig.Content.Word> words = page.GetWords();
                            string example2 = string.Join(string.Empty, words.Select(x => x.Text));

                            IEnumerable<IPdfImage> images = page.GetImages();
                            foreach (IPdfImage image in images)
                            {
                                string ext = ".jpg";
                                byte[] rawBytes = null;
                                if (image.TryGetPng(out rawBytes))
                                {
                                    ext = ".png";
                                }
                                else if (image.TryGetBytesAsMemory(out var rawBytesReadOnly))
                                {
                                    rawBytes = rawBytesReadOnly.ToArray();
                                }
                                else
                                {
                                    rawBytes = image.RawBytes.ToArray();
                                }

                                var imageFileName = fileName.Replace(".pdf", ++imageIndex + ext);
                                using (var fs = new FileStream(imageFileName, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(rawBytes, 0, rawBytes.Length);
                                }

                                sb.AppendLine(await ocrService.ReadFileLocal(imageFileName));
                            }
                        }

                        File.Delete(ocrFileName);
                        File.WriteAllText(ocrFileName, sb.ToString());

                        if (imageIndex == 0)
                            logger.LogWarning("revisar: " + fileName);
                    }
                    else
                    {
                        var ocrLines = await ocrService.ReadFileLocal(fileName);
                        File.WriteAllText(ocrFileName, ocrLines);
                    }
                }

                //using (PdfReader pdfReader = new PdfReader(fileName))
                //{
                //    if (pdfReader.NumberOfPages > 1)
                //    {
                //        File.Move(ocrFileName, fileName.Replace(".pdf", ".bkp.2.txt"));
                //    }
                //}
            }

            var lines = File.ReadAllLines(ocrFileName);

            var modo = "CABECALHO";
            var descricaoDespesa = string.Empty;
            decimal valorCalculado = 0;
            decimal valorArquivoCalculado = 0;
            var totalValidado = false;
            var despesasIncluidas = 0;
            var countValoresTotais = 0;

            foreach (var linha in lines)
            {
                //Console.WriteLine(modo + ": " + linha);

                if (modo == "CABECALHO")
                {
                    var linhaTemp = linha.ToUpper().Trim();
                    if (linhaTemp == "PENDÊNCIA DE ASSINATURA")
                    {
                        logger.LogError("Arquivo não disponivel por pendência de assinatura!");
                        return;
                    }

                    if (!linhaTemp.StartsWith("VLR R") &&
                        !linhaTemp.StartsWith("VER R") &&
                        !linhaTemp.StartsWith("VIR R") &&
                        !linhaTemp.StartsWith("VLR. R") &&
                        !linhaTemp.StartsWith("VIR. R") &&
                        !linhaTemp.StartsWith("VUR. R") &&
                        !linhaTemp.StartsWith("VALOR")) continue;

                    totalValidado = false;
                    modo = "CONTEUDO";
                    continue;
                }

                if (modo == "CONTEUDO")
                {
                    var parts = linha.Split(new char[] { ' ', '|' });
                    if (parts[0].Length < 3 && int.TryParse(parts[0], cultureInfo, out int sequencial))
                    {
                        descricaoDespesa = linha.Replace(parts[0], "").Replace("-", "").Replace("|", "").TrimStart();
                        continue;
                    }

                    if (linha.Length < 15)
                    {
                        string valor = ObterValorNumerico(linha);

                        if (decimal.TryParse(valor, cultureInfo, out decimal value))
                        {
                            if (value > 35000)
                            {
                                logger.LogError("Valor com formatação incorreta ignorado: {Valor}", value);
                                continue;
                            }

                            if (value.ToString(cultureInfo).Split(",").Length > 2)
                            {
                                logger.LogError("Valor com mais de duas casas decimais: {Valor}", value);
                            }

                            if (value > 0)
                            {
                                CamaraEstadualDespesaTemp despesaTemp = new CamaraEstadualDespesaTemp()
                                {
                                    Origem = fileName,
                                    Nome = nomeParlamentar,
                                    Ano = (short)competencia.Year,
                                    Mes = (short)competencia.Month,
                                    DataEmissao = competencia,
                                    Valor = value,
                                    TipoDespesa = ResolveDescricao(descricaoDespesa)
                                };

                                valorCalculado += value;
                                valorArquivoCalculado += value;
                                despesasIncluidas++;
                                InserirDespesaTemp(despesaTemp);
                            }

                            continue;
                        }
                    }

                    if (linha.StartsWith("TOTA", StringComparison.InvariantCultureIgnoreCase))
                    {
                        countValoresTotais++;
                        modo = "RODAPE";

                        if (linha.Contains("R$") || linha.Contains("=") || linha.Contains(":") || linha.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length == 2)
                        {
                            var valor = linha.Replace("/", "").Trim().Split(" ").Last();

                            if (valor.Length < 15 && decimal.TryParse(valor, cultureInfo, out decimal value))
                            {
                                ValidaValorTotal(fileName, valorCalculado, value, despesasIncluidas);

                                totalValidado = true;
                                valorCalculado = 0;
                                modo = "CABECALHO";
                            }
                        }

                        continue;
                    }

                    if (linha.Length < 5) continue;
                    descricaoDespesa += linha;
                }

                if (modo == "RODAPE")
                {
                    if (linha.Trim() == "R$" || linha.Trim() == "RS" || linha.StartsWith("DECLARAÇÃO E TERMO DE RESPONSABILIDADE") || linha.Length < 5) continue;

                    var valor = ObterValorNumerico(linha);

                    if (valor.Length < 15 && decimal.TryParse(valor, cultureInfo, out decimal value))
                    {
                        ValidaValorTotal(fileName, valorCalculado, value, despesasIncluidas);

                        totalValidado = true;
                    }

                    valorCalculado = 0;
                    modo = "CABECALHO";
                }
            }

            using (PdfReader pdfReader = new PdfReader(fileName))
            {
                if (pdfReader.NumberOfPages != countValoresTotais)
                {
                    logger.LogWarning("Foram encontrados apenas {NumeroDeValoresTotais} em {NumeroPaginasPdf} páginas com valor total de {ValorTotal}", countValoresTotais, pdfReader.NumberOfPages, valorArquivoCalculado);
                }
            }

            if (!totalValidado)
            {
                logger.LogError("Valor total não validado! Valor esperado: {ValorEsperado}", valorCalculado);
            }
            else if (valorArquivoCalculado < 1000)
            {
                logger.LogError("Valor total abaixo do minimo esperado: {Valor}", valorArquivoCalculado);
            }
            else if (lines.Count() < 10)
            {
                logger.LogError("Linhas do abaixo do minimo esperado: {Valor}", lines.Count());
            }

            //logger.LogInformation("{QtdItens} despesas com valor total de {Valor}", itensComValor, valorArquivoCalculado);
        }

        static string ObterValorNumerico(string linha)
        {
            var valor = linha.Replace("R$", "").Replace("RS", "").Replace("$", "").Replace("/", "").Split("-")[0].Trim().Replace(" ", ".");
            if (valor.Split(".").Length == 3)
            {
                valor = valor.ReplaceLast(".", ",");
            }
            if (valor.Split(",").Length == 3)
            {
                valor = valor.ReplaceFirst(",", ".");
            }
            if (valor.Contains(".") && !valor.Contains(",") && valor.Split(".")[1].Length <= 2)
            {
                valor = valor.Replace(".", ",");
            }

            return valor;
        }

        string ResolveDescricao(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "Indenizações e Restituições";

            var partes = texto.Split(new char[] { '.', '|' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = partes.Length - 1; i >= 0; i--)
            {
                switch (partes[i].RemoveAccents())
                {
                    // (14)
                    case string x when x.Contains("curso", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("palestra", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("seminario", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("simposio", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("congresso", StringComparison.InvariantCultureIgnoreCase):
                        return "Participação do parlamentar dos servidores lotados no seu gabinete em cursos, palestras, seminários, simpósios, congressos ou eventos congêneres";

                    // 13 (15)
                    case string x when x.Contains("correspondencia", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("Grafic", StringComparison.InvariantCultureIgnoreCase):
                        return "Portes de correspondência, registros postais, aéreos e telegramas";

                    // (12) 
                    case string x when x.Contains("seguranca", StringComparison.InvariantCultureIgnoreCase):
                        return "Serviços de segurança prestados por empresa especializada";

                    // 12 (11)
                    case string x when x.Contains("fotocopias, edicao de jornais, livros, revistas", StringComparison.InvariantCultureIgnoreCase):
                        return "Fotocópias, edição de jornais, livros, revistas e impressos e gráficos para consumo do gabinete e divulgação da atividade parlamentar";

                    // 11 (10)
                    case string x when x.Contains("producao de videos", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("contratacao de empresa especializada para producao", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("documentarios", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("midia online", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("propaganda eleitoral", StringComparison.InvariantCultureIgnoreCase):
                        return "Contratação de empresa especializada para produção de videos ou documentários para utilização na TV, em telões ou reuniões comunitárias vedadas o uso em campanha ou propaganda";
                    //          Contratação de empresa especializada para produção, exibição, disseminação conteúdo em mídia online e off-line para divulgação da atividade parlamentar vedadas o uso em campanha ou propaganda eleitoral.

                    // 10 (9)
                    case string x when x.Contains("alimentacao d", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("mantimentos", StringComparison.InvariantCultureIgnoreCase):
                        return "Alimentação do parlamentar e dos servidores lotados no seu gabinete, mesmo na Capital do Estado, quando a necessidade do apoio à atividade parlamentar ou o próprio exercício desta atividade parlamentar assim exigir";

                    // 9 (13)
                    case string x when x.Contains("software", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("internet", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("TV a cabo ou similar", StringComparison.InvariantCultureIgnoreCase):
                        return "Aquisição ou locação de software, serviços postais; assinaturas de jornais, revistas, livros, publicações, TV a cabo ou similar e de acesso à internet.";

                    // 8
                    case string x when x.Contains("material de expediente", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("informatica", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("mantimentos para gabinete", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("materiais para ornamentacao", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("material de limpeza", StringComparison.InvariantCultureIgnoreCase):
                        return "Aquisição de material de expediente e suprimentos de informática";
                    // Materiais para ornamentação de escritório para gabinete.

                    // 7
                    case string x when x.Contains("atividade parlamentar", StringComparison.InvariantCultureIgnoreCase) || x.Contains("atividade parlementar", StringComparison.InvariantCultureIgnoreCase)
                        || (x.Contains("publicacao", StringComparison.InvariantCultureIgnoreCase) && x.Contains("periodicos", StringComparison.InvariantCultureIgnoreCase))
                        || x.Contains("disciplinadas pela legislacao eleitoral", StringComparison.InvariantCultureIgnoreCase):
                        return "Divulgação da atividade parlamentar em todas as modalidades de mídia, observando-se as restrições disciplinadas pela legislação eleitoral";

                    // [6]
                    case string x when x.Contains("servicos juridicos", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("assessoria juridica", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("consulta juridica", StringComparison.InvariantCultureIgnoreCase):
                        return "Serviços Jurídicos especializado";

                    // 6 [4]
                    case string x when x.Contains("assessoria", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("consultoria", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("publicidade", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("pesquisas", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("especializados", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("projetos sociais", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("apoio a atividade", StringComparison.InvariantCultureIgnoreCase):
                        return "Contratação, para fins de apoio à atividade parlamentar, de empresas de consultoria, assessorias, elaboração de projetos sociais, pesquisas e outros trabalhos técnicos especializados";
                    //          Serviço Especializado Publicidade/Propaganda

                    // [5] 
                    case string x when x.Contains("graficos", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("impressora", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("papelaria", StringComparison.InvariantCultureIgnoreCase):
                        return "Serviços e materiais Gráficos";
                    //          Impressora e papelaria.

                    // 5
                    case string x when x.Contains("combustive", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("reparacao de veiculos", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("lubrificantes", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("veiculos proprios", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("contratados de terceiros, utilizados para o apoio", StringComparison.InvariantCultureIgnoreCase):
                        return "Combustíveis, lubrificantes, seguros, peças de reposição e reparação de veículos próprios ou contratados de terceiros, utilizados para o apoio ou exercício da atividade parlamentar";

                    // 4
                    case string x when x.Contains("locacao de automoveis", StringComparison.InvariantCultureIgnoreCase):
                        return "Locação de automóveis de Pessoas Jurídicas";

                    // 3
                    case string x when x.Contains("locomocao do parlamentar", StringComparison.InvariantCultureIgnoreCase)
                        || x.StartsWith("Hospedagem", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("passagens", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("passagem", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("fretamento de aeronaves", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("locacao do parlamentar e dos servidores lotados", StringComparison.InvariantCultureIgnoreCase) // Erro de OCR
                        || x.StartsWith("servicos de taxi, uber ou similares, pedagio ou estacionamento", StringComparison.InvariantCultureIgnoreCase):
                        return "Locomoção do parlamentar e dos servidores lotados em seu gabinete, compreendendo passagens, hospedagem, alimentação e locação de meios de transporte";
                    //          Locomoção do parlamentar e dos servidores lotados em seu gabinete, compreendendo passagens, locação ou fretamento de aeronaves ou fretamentos de veículos automotores, locação ou fretamento de embarcações, serviços de taxi, uber ou similares, pedágio ou estacionamento, hospedagem, alimentação e locação de meios de transporte com ou sem condutor.

                    // 2
                    case string x when x.Contains("telefon", StringComparison.InvariantCultureIgnoreCase):
                        return "Uso de telefone fixo ou móvel que esteja a sendo utilizado pelo Parlamentar ou por servidor lotado em seu gabinete para o apoio ou exercício da atividade parlamentar";
                    //          Uso de telefone fixo ou móvel inclusive com plano de dados, que esteja sendo utilizado pelo Parlamentar ou por servidor lotado em seu gabinete para o apolo ou exercício da atividade parlamentar.

                    // 1
                    case string x when x.StartsWith("Locacao de imoveis", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("iptu", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("taxas condominiais", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("escritorio de apoio", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("energia eletrica", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("manutencao", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("aluguel escritorio", StringComparison.InvariantCultureIgnoreCase)
                        || x.Contains("apoio ao exercicio da atividade", StringComparison.InvariantCultureIgnoreCase):
                        return "Locação de imóveis, máquinas, equipamentos e utensílios utilizados exclusivamente em escritório de apoio ao exercício da atividade parlamentar, inclusive taxas condominiais, Imposto Predial e Territorial Urbano - IPTU, Taxas de Corpo de Bombeiros, consumo de água e energia elétrica e outras despesas de manutenção e conservação dos referidos bens móveis ou imóveis";
                    //          Locação de imóveis, ou mesmo contratação de espaço compartilhado de trabalho, na modalidade coworking, incluindo os serviços indispensáveis ao funcionamento da unidade, locação de máquinas, equipamentos e utensilios utilizados exclusivamente em escritório de apoio ao exercício da atividade parlamentar, inclusive taxas condominiais, Imposto Predial e Territorial Urbano - IPTU, Taxas de Corpo de Bombeiros, seguro contra incêndio, consumo de água, despesa com esgoto e energia elétrica e outras despesas de manutenção e conservação dos referidos bens móveis ou imóveis
                    //          Manutenção gabinete (serviço de pintura- material e mão de obra)

                    case string x when x.Contains("outras despesas", StringComparison.InvariantCultureIgnoreCase):
                        return "Indenizações e Restituições";
                }
            }

            logger.LogError("Descrição '{Descricao}' não localizada!", texto);
            return texto;
        }

        int ResolveMes(string mes) => mes.Trim().ToLower() switch
        {
            "janeiro" => 1,
            "fevereiro" => 2,
            "março" => 3,
            "abril" => 4,
            "maio" => 5,
            "junho" => 6,
            "julho" => 7,
            "agosto" => 8,
            "setembro" => 9,
            "outubro" => 10,
            "novembro" => 11,
            "dezembro" => 12,
            "dudu ronalsa" => 9, // Invalid name: https://www.al.al.leg.br/transparencia/orcamento-e-financas/viap-verba-indenizatoria-de-atividade-parlamentar/2025/dudu-ronalsa
            _ => throw new ArgumentOutOfRangeException(nameof(mes), $"Mês invalido: {mes}"),
        };


        //private void ImportarEmpenhosParaComparacao(IBrowsingContext context, int ano)
        //{
        //    var i = 0;
        //    var NomeFavorecido = i++;
        //    var CNPJCPF = i++;
        //    var Objeto = i++;
        //    var TipoLicitacao = i++;
        //    var NumeroEmpenho = i++;
        //    var Data = i++;
        //    var ValorEmpenhado = i++;
        //    var ValorPago = i++;

        //    var document = context.OpenAsyncAutoRetry($"https://www.al.al.leg.br/transparencia/orcamento-e-financas/empenhos-e-pagamentos/{ano}").GetAwaiter().GetResult();

        //    var meses = document.QuerySelectorAll("#content-core .headline a");
        //    foreach (var mes in meses)
        //    {
        //        var urlEmpenhosDoMes = mes.Attributes["href"].Value;

        //        using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes.TextContent }))
        //        {
        //            var competencia = new DateOnly(ano, ResolveMes(mes.TextContent), 1);

        //            var subdocument = context.OpenAsyncAutoRetry(urlEmpenhosDoMes).GetAwaiter().GetResult();
        //            var empenhos = subdocument.QuerySelectorAll("#content-core .listing tbody tr");

        //            foreach (var empenho in empenhos)
        //            {
        //                var linhas = empenho.QuerySelectorAll("td");

        //                var nome = linhas[NomeFavorecido].TextContent.Trim();
        //                if (string.IsNullOrEmpty(nome) || nome.StartsWith("MÊS") || nome == "Nome do Favorecido" || nome == "ASSEMBLEIA LEGISLATIVA ESTADUAL") continue;
        //                if (linhas[Objeto].TextContent.Trim() != "IDENIZAÇÃO DE DESPESA COM O GABINETE") continue;
        //                if (linhas[ValorEmpenhado].TextContent.Trim() == "0,00") continue;

        //                var data = linhas[Data].TextContent.Trim().Replace(".20233", ".2023");
        //                if (data == "27.032.2023") data = "27/02/2023";
        //                else if (data == "2808/2023") data = "28/08/2023";
        //                else if (data == "72018/12") data = $"18/12/{ano}";
        //                else if (data.EndsWith("-abr")) data = $"{data.Replace("-abr", "/04")}/{ano}";
        //                else if (data.EndsWith("-mai")) data = $"{data.Replace("-mai", "/05")}/{ano}";
        //                else if (data.EndsWith("/ago")) data = $"{data.Replace("/ago", "/08")}/{ano}";
        //                else if (data.EndsWith("/set")) data = $"{data.Replace("/set", "/09")}/{ano}";

        //                var empenhoTemp = new DeputadoEstadualEmpenhoTemp();
        //                empenhoTemp.Competencia = competencia;
        //                empenhoTemp.NomeFavorecido = nome;
        //                empenhoTemp.CNPJCPF = linhas[CNPJCPF].TextContent.Trim();
        //                //empenhoTemp.Objeto = linhas[Objeto].TextContent;
        //                //empenhoTemp.TipoLicitacao = linhas[TipoLicitacao].TextContent;
        //                empenhoTemp.NumeroEmpenho = linhas[NumeroEmpenho].TextContent.Trim();
        //                empenhoTemp.Data = DateOnly.Parse(data, cultureInfo);
        //                empenhoTemp.ValorEmpenhado = Convert.ToDecimal(linhas[ValorEmpenhado].TextContent.Trim().Replace("39,24,83", "3924,83"), cultureInfo);
        //                empenhoTemp.ValorPago = Convert.ToDecimal(linhas[ValorPago].TextContent.Trim().Replace("39,24,83", "3924,83"), cultureInfo);

        //                repositoryService.Insert(empenhoTemp);
        //            }
        //        }
        //    }
        //}

        public override void ValidaImportacao(int ano)
        {
            base.ValidaImportacao(ano);

            connection.Execute(@"
UPDATE temp.cl_empenho_temp SET nome_deputado = nome_favorecido WHERE nome_favorecido NOT ILIKE '%DEPUTADO%';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'RONALDO MEDEIROS' WHERE nome_favorecido ILIKE 'AMANDA DA SILVA FERRAZ (DELEGATÁRIA DO DEPUTADO RONALDO MEDEIROS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'YVAN BELTRÃO' WHERE nome_favorecido ILIKE 'ANDERSON RONDINELLY LIRA PALMEIRA (DELEGATÁRIO DO DEPUTADO YVAN BELTRÃO)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'GALBA NOVAES' WHERE nome_favorecido ILIKE 'ANTONIO FARIAS DA SILVA JUNIOR (DELEGATÁRIO DO DEPUTADO GALBA NOVAES)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'GALBA NOVAES' WHERE nome_favorecido ILIKE 'ANTONIO FARIAS DA SILVA JUNIOR (DELEGATÁRIO DO DEPUTADO GALBA NOVAIS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ALEXANDRE AYRES' WHERE nome_favorecido ILIKE 'CAIO QUINTELLA JUCÁ DUARTE (DELEGATÁRIO DO DEPUTADO ALEXANDRE AYRES)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'DAVI MAIA' WHERE nome_favorecido ILIKE 'CLAUDIA MARQUES FREIRE (DELEGATÁRIA DO DEPUTADO DAVI MAIA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'GALBA NOVAES' WHERE nome_favorecido ILIKE 'DANIEL HENRIQUE NOVASI DE OLIVEIRA (DELEGATÁRIO DO DEPUTADO GALBA NOVAIS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'JAIR LIRA' WHERE nome_favorecido ILIKE 'DIEGO MELO FREITAS (DELEGATÁRIO DO DEPUTADO JAIR LIRA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'MESAQUE PADILHA' WHERE nome_favorecido ILIKE 'DOUGLAS DOS SANTOS SILVA (DELEGATÁRIO DO DEPUTADO MESAQUE PADILHA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANDRÉ LUIZ' WHERE nome_favorecido ILIKE 'FABIANO GOMES DE SOUZA (DELEGATÁRIO DO DEPUTADO ANDRÉ LUIZ)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANDRÉ SILVA' WHERE nome_favorecido ILIKE 'FABIANO GOMES DE SOUZA (DELEGATÁRIO DO DEPUTADO ANDRÉ SILVA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'HENRIQUE CHICAO' WHERE nome_favorecido ILIKE 'FABIANO GOMES DE SOUZA (DELEGATÁRIO DO DEPUTADO HENRIQUE CHICAO)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANDRÉ SILVA' WHERE nome_favorecido ILIKE 'FABRICIO GOMES DE SOUZA (DELEGATÁRIO DO DEPUTADO ANDRÉ SILVA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'DUDU RONALSA' WHERE nome_favorecido ILIKE 'FERNANDO PRIMOLA PEDROSA CARVALHO (DELEGATÁRIO DO DEPUTADO DUDU RONALSA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'Leonam Pinheiro Rodrigues' WHERE nome_favorecido ILIKE 'FÁBIO MALTA ALCANTARA RODRIGUES DE LIMA (DELEGATÁRIO DO DEPUTADO LEONAM PINHEIRO)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'GILVAN BARROS' WHERE nome_favorecido ILIKE 'GERÔNIMO BEZERRA (DELEGATÁRIO DO DEPUTADO GILVAN BARROS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'INÁCIO LOIOLA' WHERE nome_favorecido ILIKE 'HERMANN JOSÉ DE AMORIM VASCONCELOS (DELEGATÁRIO DO DEPUTADO INÁCIO LOIOLA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'BRUNO TOLEDO' WHERE nome_favorecido ILIKE 'IRIS DA SILVA GOUVEIA (DELEGATÁRIA DO DEPUTADO BRUNO TOLEDO)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANDRÉ SILVA' WHERE nome_favorecido ILIKE 'JOÃO GABRIEL GAIA GOMES (DELEGATARIO DO DEPUTADO ANDRÉ SILVA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'MARCELO VICTOR' WHERE nome_favorecido ILIKE 'KLAYDSON RYTHCHARDSON MARQUES SILVA (DELEGATÁRIO DO DEPUTADO MARCELO VICTOR)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'RICARDO PEREIRA' WHERE nome_favorecido ILIKE 'MANOEL ANGELINO DA SILVA (DELEGATÁRIO DO DEPUTADO RICARDO PEREIRA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'TARCIZO FREIRE' WHERE nome_favorecido ILIKE 'MICHAEL VIEIRA DANTAS (DELEGATÁRIO DO DEPUTADO TARCIZO FREIRE)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'MARCOS BARBOSA' WHERE nome_favorecido ILIKE 'SAULO DE TACIO FERNANDES GOMES DA COSTA (DELEGATÁRIO DO DEPUTADO MARCOS BARBOSA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'REMI CALHEIROS' WHERE nome_favorecido ILIKE 'SHIRLEY RIBEIRO MELO DE OLIVEIRA SILVA(DELEGATÁRIA DEPUTADO REMI CALHEIROS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'REMI CALHEIROS' WHERE nome_favorecido ILIKE 'FRANCISCO  JOSÉ DA SILVA (DELEGATÁRIO REMI CALHEIROS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'BRENO ALBUQUERQUE' WHERE nome_favorecido ILIKE 'SILVANA MARIA BARBOSA GOMES DE MELO (DELEGATÁRIA DO DEPUTADO BRENO ALBUQUERQUE)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'MARCELO VICTOR' WHERE nome_favorecido ILIKE 'THIAGO PIMENTEL LEITE TEIXEIRA (DELEGATÁRIO DO DEPUTADO MARCELO VICTOR)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'SÂMIA MASCARENHAS' WHERE nome_favorecido ILIKE 'ANA CLAUDIA BEZERRA(DELEGATÁRIA DA DEPUTADA SAMIA VASCONCELOS';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'SÂMIA MASCARENHAS' WHERE nome_favorecido ILIKE 'ANA CLAUDIA BEZERRA(DELEGATÁRIA DA DEPUTADA SÂMIA MASCARENHAS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'CIBELE MOURA' WHERE nome_favorecido ILIKE 'CHRISTIANO HENRIQUE NASCIMENTO FARIAS  (DELEGATÁRIO DO DEPUTADA CIBELE MOURA)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANGELA CANUTO' WHERE nome_favorecido ILIKE 'CLAUDIA KALINE DE FRAIAS LARGES TORRES (DELEGATÁRIA DO DEPUTADA ANGELA CANUTO)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANGELA GARROTE' WHERE nome_favorecido ILIKE 'ENIRALDO RIBEIRO BALBINO (DELEGATÁRIO DA DEPUTADA ANGELA GARROTE)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'FLAVIA CAVALCANTE' WHERE nome_favorecido ILIKE 'MIRELLA DE LIMA GOMES REGO (DELEGATÁRIA DA DEPUTADA (FLAVIA CAVALCANTE)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ROSE DAVINO' WHERE nome_favorecido ILIKE 'RALPH DA CRUZ ALBERMAZ (DELEGATÁRIO DA DEPUTADA ROSE DAVINO)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'FLAVIA CAVALCANTE' WHERE nome_favorecido ILIKE 'MIRELLA DE LIMA GOMES REGO (DELEGATÁRIA FLAVIA    CAVALCANTE';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANGELA GARROTE' WHERE nome_favorecido ILIKE 'RALPH DA CRUZ ALBERNAZ(DELEGATÁRIO ANGELA GARROTE)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ROSE DAVINO' WHERE nome_favorecido ILIKE 'RALPH DA CRUZ ALBERNAZ(DELEGATÁRIO ROSE DAVINO';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'GABRIELA GONÇALVES' WHERE nome_favorecido ILIKE 'VICTOR LIMA ALBUQUERQUE (DELEGATÁRIO GABRIELA GONÇALVES)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'REMI CALHEIROS' WHERE nome_favorecido ILIKE 'FRANCISCO  JOSÉ DA SILVA (DELEGATÁRIO REMI CALHEIROS)';
UPDATE temp.cl_empenho_temp SET nome_deputado = 'ANTONIO RIBEIRO DE ALBUQUERQUE' where nome_deputado = 'ANTONIO  RIBEIRO DE ALBUQUERQUE';");


        }

        private class ParlamentaresPR
        {
            public string status { get; set; }
            public string content { get; set; }
            public bool isLastPage { get; set; }
        }

        private class Fotos
        {
            public string pathXxlarge { get; set; }
            public string name { get; set; }
        }
    }
}
