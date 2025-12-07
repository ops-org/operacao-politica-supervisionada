using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.ALE.Comum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado do Ceará
/// https://al.ce.gov.br/
/// </summary>
public class Ceara : ImportadorBase
{
    public Ceara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarCeara(serviceProvider);
        importadorDespesas = new ImportadorDespesasCeara(serviceProvider);
    }
}

public class ImportadorDespesasCeara : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasCeara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.ce.gov.br/index.php/despesas/despesas-alece/verba-de-desempenho-parlamentar",
            Estado = Estado.Ceara,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();

        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");

        var dcForm = new Dictionary<string, string>();
        dcForm.Add("opcao", "1");
        dcForm.Add("mes", mes.ToString("00"));
        dcForm.Add("ano", ano.ToString());
        dcForm.Add("nome", "");
        document = form.SubmitAsyncAutoRetry(dcForm, true).GetAwaiter().GetResult();

        var deputados = document.QuerySelectorAll("table#table tbody tr");
        foreach (var deputado in deputados)
        {
            var colunas = deputado.QuerySelectorAll("td");
            var nomeParlamentar = colunas.First().TextContent.Trim();
            if(string.IsNullOrEmpty(nomeParlamentar)) continue;

            var linkDetalhes = colunas.ElementAt(1).QuerySelector("a").Attributes["onclick"].Text();

            int srcIndex = linkDetalhes.IndexOf(".src='");
            if (srcIndex != -1)
            {
                int startIndex = srcIndex + 6; // Length of ".src='"
                int endIndex = linkDetalhes.IndexOf("'", startIndex);

                linkDetalhes = linkDetalhes.Substring(startIndex, endIndex - startIndex);
            }

            var documentDetalhes = context.OpenAsyncAutoRetry(linkDetalhes).GetAwaiter().GetResult();
            var despesas = documentDetalhes.QuerySelectorAll("table#table tbody tr");
            foreach (var despesa in despesas)
            {
                var colunasDespesa = despesa.QuerySelectorAll("td");

                CamaraEstadualDespesaTemp despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Documento = colunasDespesa.ElementAt(0).TextContent,
                    Observacao = colunasDespesa.ElementAt(1).TextContent,
                    CnpjCpf = colunasDespesa.ElementAt(2).TextContent,
                    Empresa = colunasDespesa.ElementAt(3).TextContent,
                    Valor = Convert.ToDecimal(colunasDespesa.ElementAt(4).TextContent, cultureInfo),
                    Ano = (short)ano,
                    Mes = (short)mes,
                    DataEmissao = new DateTime(ano, mes, 1),
                    TipoDespesa = ObterTipoDespesa(colunasDespesa.ElementAt(1).TextContent),
                    Nome = CorrigeNomeParlamentar(nomeParlamentar)
                };

                InserirDespesaTemp(despesaTemp);
            }
        }
    }

    private string CorrigeNomeParlamentar(string nome)
    {
        nome = nome.Replace("DEP", "")
            //.Replace(".", "")
            //.Replace("Por Solicitacao do Utado", "")
            //.Replace("Por Solicitacao da Utada", "")
            //.Split("-")[0]
            .Trim();

        //nome = nome.ToUpper() switch
        //{
        //    "PLANO DE SAUDE" => string.Empty,
        //    "225/03" => string.Empty, // TODO: Arquivo 2023-07
        //    _ => nome
        //};

        return nome.ToTitleCase();
    }

    private string ObterTipoDespesa(string tipo)
    {
        switch (tipo)
        {
            case "ALIMENTAÇÃO": return "Alimentação";
            case "ASSESSORIA": return "Assessoria e Consultoria";
            case "COMBUSTÍVEIS": return "Combustíveis";
            case "CONSULTORIA": return "Assessoria e Consultoria";
            case "DIVULGAÇÃO DAS ATIVIDADES PARLAMENTARES": return "Divulgação das Atividades Parlamentares";
            case "INTERNET": return "Internet e TV";
            case "LOCAÇÃO DO VEÍCULO": return "Locação de veículos";
            case "LOCAÇÃO DOS VEÍCULOS": return "Locação de veículos";
            case "MANUTECAO DE SITE": return "Hospedagem, Atualização e Manutenção de Sites";
            case "PASSAGENS AÉREAS": return "Passagens Aéreas";
            case "PASSAGENS TERRESTRES": return "Passagens Terrestres";
            case "PESQUISA DE OPINIÃO": return "Pesquisa de Opinião Pública";
            case "PLANO DE SAÚDE": return "Plano de Saúde";
            case "PLANO DE SAUDE": return "Plano de Saúde";
            case "SEGURO DE VIDA": return "Seguro de Vida";
            case "SERVIÇOS DE HOSPEDAGEM": return "Serviços de Hospedagem";
            case "SERVIÇOS GRÁFICOS": return "Serviços Gráficos";
            case "SERVIÇOS POSTAIS": return "Serviços Postais";
            case "TELEFONIA": return "Telefonia";
            case "TV": return "Internet e TV";
        }

        switch (tipo)
        {
            case string t when t.Contains("REFEICAO") ||
                t.Contains("REFEIÇÃO") ||
                t.Contains("ALIMENTACAO") ||
                t.Contains("ALIMENTAÇÃO"):
                return "Alimentação";

            case string t when t.Contains("ABASTECIMENTO DE COMBUSTIVEIS") ||
                t.Contains("ABASTECIMENTO DE COMBUSTÍVEIS") ||
                t.Contains("COMBIUSTIVEIS"):
                return "Combustíveis";

            case string t when t.Contains("CONSULTORIA") ||
                t.Contains("ASSESSORIA") ||
                t.Contains("ASSESORIA") ||
                t.Contains("ACOMPANHAMENTO") ||
                t.Contains("RECURSOS CONSIGNADOS NO ORCAMENTO"):
                return "Assessoria e Consultoria";

            case string t when t.Contains("TELEFONIA") ||
                t.Contains("TELECOMUNICAÇÕES") ||
                t.Contains("TELECOMUNICACOES") ||
                t.Contains("INTERNET") ||
                t.Contains("INTERTNET") ||
                t.Contains("BANDA LARGA") ||
                t.Contains("TV") ||
                t.Contains("MULTIMIDIA") ||
                t.Contains("MULTIMÍDIA") ||
                t.Contains("ASSINATURA"):
                return "Internet e TV";

            case string t when t.Contains("FRETAMENTO DA AERONAVE") ||
                t.Contains("FRETAMENTO DE HELICOPTERO"):
                return "Fretamento de Aeronaves";

            case string t when t.Contains("HOSPEDAGEM") ||
                t.Contains("HOSPEDAGENS"):
                return "Serviços de Hospedagem";

            case string t when t.Contains("PASSAGENS TERRESTRES") ||
                t.Contains("VALE TRANSPORTE"):
                return "Passagens Terrestres";

            case string t when t.Contains("PASSAGENS AEREAS") ||
                t.Contains("PASSAGENS AÉREAS") ||
                t.Contains("PASSAGEM AEREA") ||
                t.Contains("PASSAGEM AÉREA") ||
                t.Contains("PASSAGEM ÁEREA"):
                return "Passagens Aéreas";

            case string t when t.Contains("GRAFICOS") ||
                t.Contains("GRÁFICOS"):
                return "Serviços Gráficos";

            case string t when t.Contains("ATIVIDADES PARLAMENTARES") ||
                t.Contains("ATIVIDADE PARLAMENTAR") ||
                t.Contains("ATIVIDADES ARLAMENTARES") ||
                t.Contains("VEICULAÇÃO DE MÍDIAS") ||
                t.Contains("DIVULGACAO") ||
                t.Contains("DIVULGAÇÃO") ||
                t.Contains("PUBLICAÇÃO") ||
                t.Contains("PUBLICACAO") ||
                t.Contains("COMUNICACAO") ||
                t.Contains("COMUNICAÇÃO"):
                return "Divulgação das Atividades Parlamentares";

            case string t when t.Contains("PLANO DE SAUDE") ||
                t.Contains("PLANO DE SAÚDE") ||
                t.Contains("PLANO DE SDAUDE"):
                return "Plano de Saúde";

            case string t when t.Contains("SEGURO DE VIDA") ||
                t.Contains("SEGURO DEVIDA"):
                return "Seguro de Vida";

            case string t when t.Contains("SITE") ||
                t.Contains("SITIO") ||
                t.Contains("MANUTENÇÃO DO SÍTIO"):
                return "Hospedagem, Atualização e Manutenção de Sites";

            case string t when t.Contains("PESQUISA") ||
                t.Contains("OPINIAO PUBLICA") ||
                t.Contains("OPINIÃO PÚBLICA"):
                return "Pesquisa de Opinião Pública";

            case string t when t.Contains("POSTAIS"): return "Serviços Postais";

            case string t when t.Contains("VEICULO") ||
                t.Contains("VEÍCULO") ||
                t.Contains("LOCACAO") ||
                t.Contains("LOCAÇÃO"):
                return "Locação de veículos";
        }

        return "Indenizações e Restituições";
    }
}

//public class ImportadorDespesasCeara : ImportadorDespesasArquivo
//{
//    public ImportadorDespesasCeara(IServiceProvider serviceProvider) : base(serviceProvider)
//    {
//        config = new ImportadorCotaParlamentarBaseConfig()
//        {
//            BaseAddress = "https://transparencia.al.ce.gov.br/",
//            Estado = Estado.Ceara,
//            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
//        };
//    }

//    /// <summary>
//    /// Arquivos disponiveis anualmente a partir de 2021
//    /// https://transparencia.al.ce.gov.br/index.php/despesas/despesas-alece/verba-de-desempenho-parlamentar
//    /// </summary>
//    /// <param name="ano"></param>
//    /// <returns></returns>
//    public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
//    {
//        var currentDate = DateTime.Today;
//        Dictionary<string, string> arquivos = new();

//        for (int mes = 1; mes <= 12; mes++)
//        {
//            if (currentDate.Year == ano && mes > currentDate.Month) break;

//            var base64 = Utils.EncodeTo64($"{mes:00}|{ano}|");
//            var urlOrigem = $"{config.BaseAddress}includes/verba_de_desempenho_parlamentar_csv.php?codigo={base64}";
//            var caminhoArquivo = $"{tempPath}/CLCE-{ano}-{mes}.csv";

//            //if (DateTime.Now.AddMonths(-2).Date >= new DateTime(ano, mes, 1) && File.Exists(caminhoArquivo))
//            //{
//            //    if ((new FileInfo(caminhoArquivo)).CreationTime <= currentDate.AddDays(-7))
//            //        File.Delete(caminhoArquivo);
//            //}

//            arquivos.Add(urlOrigem, caminhoArquivo);
//        }

//        return arquivos;
//    }

//    public override void ImportarDespesas(string caminhoArquivo, int ano)
//    {
//        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

//        int indice = 0;
//        int Verba = indice++;
//        int Descricao = indice++;
//        int Conta = indice++;
//        int Favorecido = indice++;
//        int Trecho = indice++;
//        int Vencimento = indice++;
//        int Valor = indice++;

//        var linha = 0;
//        string nomeParlamentar = null;
//        decimal valorTotalDeputado = 0;
//        var despesasIncluidas = 0;
//        short anoDespesa = 0;
//        DateTime dataEmissao = DateTime.MinValue;
//        foreach (string line in File.ReadLines(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
//        {
//            if (line.StartsWith("TOTAL GERAL"))
//            {
//                linha = 0;

//                var valorTotalArquivo = Convert.ToDecimal(Convert.ToDecimal(line.Split(';')[4], cultureInfo), cultureInfo);
//                ValidaValorTotal(valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

//                continue;
//            }
//            linha++;

//            if (linha == 1)
//            {
//                nomeParlamentar = CorrigeNomeParlamentar(line);
//                anoDespesa = (short)ano;
//                valorTotalDeputado = 0;
//                continue;
//            }

//            if (linha == 2)
//            {
//                dataEmissao = Convert.ToDateTime("01/" + line.Replace("Mes/Ano:", "").Trim(), cultureInfo);
//                continue;
//            }

//            if (linha == 3) continue; // EMPENHO;DESCRIÇÃO;CNPJ;CREDOR;VALOR
//            if (string.IsNullOrEmpty(nomeParlamentar)) continue;

//            var colunas = line.Split(';');
//            if (colunas.Length != 6) // Finaliza com ;
//            {
//                logger.LogError("Linha {Linha} invalida em {Arquivo}", line, caminhoArquivo);
//                continue;
//            }

//            var despesaTemp = new CamaraEstadualDespesaTemp();
//            despesaTemp.Nome = nomeParlamentar;
//            despesaTemp.Ano = anoDespesa;
//            despesaTemp.DataEmissao = dataEmissao;
//            despesaTemp.TipoDespesa = ObterTipoDespesa(colunas[1].Trim());
//            despesaTemp.Documento = colunas[0].Trim();
//            despesaTemp.Observacao = colunas[1].Trim();
//            despesaTemp.CnpjCpf = colunas[2].Trim();
//            despesaTemp.Empresa = colunas[3].Trim();
//            despesaTemp.Valor = Convert.ToDecimal(colunas[4], cultureInfo);

//            InserirDespesaTemp(despesaTemp);
//            valorTotalDeputado += despesaTemp.Valor;
//            despesasIncluidas++;
//        }
//    }

//    private string CorrigeNomeParlamentar(string nome)
//    {
//        nome = nome.Replace("DEP", "")
//            .Replace(".", "")
//            .Replace("Por Solicitacao do Utado", "")
//            .Replace("Por Solicitacao da Utada", "")
//            .Split("-")[0]
//            .Trim();

//        nome = nome.ToUpper() switch
//        {
//            "PLANO DE SAUDE" => string.Empty,
//            "225/03" => string.Empty, // TODO: Arquivo 2023-07
//            _ => nome
//        };

//        return nome.ToTitleCase();
//    }

//    private string ObterTipoDespesa(string tipo)
//    {
//        switch (tipo)
//        {
//            case "ALIMENTAÇÃO": return "Alimentação";
//            case "ASSESSORIA": return "Assessoria e Consultoria";
//            case "COMBUSTÍVEIS": return "Combustíveis";
//            case "CONSULTORIA": return "Assessoria e Consultoria";
//            case "DIVULGAÇÃO DAS ATIVIDADES PARLAMENTARES": return "Divulgação das Atividades Parlamentares";
//            case "INTERNET": return "Internet e TV";
//            case "LOCAÇÃO DO VEÍCULO": return "Locação de veículos";
//            case "LOCAÇÃO DOS VEÍCULOS": return "Locação de veículos";
//            case "MANUTECAO DE SITE": return "Hospedagem, Atualização e Manutenção de Sites";
//            case "PASSAGENS AÉREAS": return "Passagens Aéreas";
//            case "PASSAGENS TERRESTRES": return "Passagens Terrestres";
//            case "PESQUISA DE OPINIÃO": return "Pesquisa de Opinião Pública";
//            case "PLANO DE SAÚDE": return "Plano de Saúde";
//            case "PLANO DE SAUDE": return "Plano de Saúde";
//            case "SEGURO DE VIDA": return "Seguro de Vida";
//            case "SERVIÇOS DE HOSPEDAGEM": return "Serviços de Hospedagem";
//            case "SERVIÇOS GRÁFICOS": return "Serviços Gráficos";
//            case "SERVIÇOS POSTAIS": return "Serviços Postais";
//            case "TELEFONIA": return "Telefonia";
//            case "TV": return "Internet e TV";
//        }

//        switch (tipo)
//        {
//            case string t when t.Contains("REFEICAO") ||
//                t.Contains("REFEIÇÃO") ||
//                t.Contains("ALIMENTACAO") ||
//                t.Contains("ALIMENTAÇÃO"):
//                return "Alimentação";

//            case string t when t.Contains("ABASTECIMENTO DE COMBUSTIVEIS") ||
//                t.Contains("ABASTECIMENTO DE COMBUSTÍVEIS") ||
//                t.Contains("COMBIUSTIVEIS"):
//                return "Combustíveis";

//            case string t when t.Contains("CONSULTORIA") ||
//                t.Contains("ASSESSORIA") ||
//                t.Contains("ASSESORIA") ||
//                t.Contains("ACOMPANHAMENTO") ||
//                t.Contains("RECURSOS CONSIGNADOS NO ORCAMENTO"):
//                return "Assessoria e Consultoria";

//            case string t when t.Contains("TELEFONIA") ||
//                t.Contains("TELECOMUNICAÇÕES") ||
//                t.Contains("TELECOMUNICACOES") ||
//                t.Contains("INTERNET") ||
//                t.Contains("INTERTNET") ||
//                t.Contains("BANDA LARGA") ||
//                t.Contains("TV") ||
//                t.Contains("MULTIMIDIA") ||
//                t.Contains("MULTIMÍDIA") ||
//                t.Contains("ASSINATURA"):
//                return "Internet e TV";

//            case string t when t.Contains("FRETAMENTO DA AERONAVE") ||
//                t.Contains("FRETAMENTO DE HELICOPTERO"):
//                return "Fretamento de Aeronaves";

//            case string t when t.Contains("HOSPEDAGEM") ||
//                t.Contains("HOSPEDAGENS"):
//                return "Serviços de Hospedagem";

//            case string t when t.Contains("PASSAGENS TERRESTRES") ||
//                t.Contains("VALE TRANSPORTE"):
//                return "Passagens Terrestres";

//            case string t when t.Contains("PASSAGENS AEREAS") ||
//                t.Contains("PASSAGENS AÉREAS") ||
//                t.Contains("PASSAGEM AEREA") ||
//                t.Contains("PASSAGEM AÉREA") ||
//                t.Contains("PASSAGEM ÁEREA"):
//                return "Passagens Aéreas";

//            case string t when t.Contains("GRAFICOS") ||
//                t.Contains("GRÁFICOS"):
//                return "Serviços Gráficos";

//            case string t when t.Contains("ATIVIDADES PARLAMENTARES") ||
//                t.Contains("ATIVIDADE PARLAMENTAR") ||
//                t.Contains("ATIVIDADES ARLAMENTARES") ||
//                t.Contains("VEICULAÇÃO DE MÍDIAS") ||
//                t.Contains("DIVULGACAO") ||
//                t.Contains("DIVULGAÇÃO") ||
//                t.Contains("PUBLICAÇÃO") ||
//                t.Contains("PUBLICACAO") ||
//                t.Contains("COMUNICACAO") ||
//                t.Contains("COMUNICAÇÃO"):
//                return "Divulgação das Atividades Parlamentares";

//            case string t when t.Contains("PLANO DE SAUDE") ||
//                t.Contains("PLANO DE SAÚDE") ||
//                t.Contains("PLANO DE SDAUDE"):
//                return "Plano de Saúde";

//            case string t when t.Contains("SEGURO DE VIDA") ||
//                t.Contains("SEGURO DEVIDA"):
//                return "Seguro de Vida";

//            case string t when t.Contains("SITE") ||
//                t.Contains("SITIO") ||
//                t.Contains("MANUTENÇÃO DO SÍTIO"):
//                return "Hospedagem, Atualização e Manutenção de Sites";

//            case string t when t.Contains("PESQUISA") ||
//                t.Contains("OPINIAO PUBLICA") ||
//                t.Contains("OPINIÃO PÚBLICA"):
//                return "Pesquisa de Opinião Pública";

//            case string t when t.Contains("POSTAIS"): return "Serviços Postais";

//            case string t when t.Contains("VEICULO") ||
//                t.Contains("VEÍCULO") ||
//                t.Contains("LOCACAO") ||
//                t.Contains("LOCAÇÃO"):
//                return "Locação de veículos";
//        }

//        return "Indenizações e Restituições";
//    }
//}

public class ImportadorParlamentarCeara : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarCeara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://al.ce.gov.br/deputados",
            SeletorListaParlamentares = ".deputado_page .deputado_card",
            Estado = Estado.Ceara,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var nomeparlamentar = parlamentar.QuerySelector(".deputado_card--nome").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (parlamentar.QuerySelector(".deputado_card--nome a") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (parlamentar.QuerySelector("img") as IHtmlImageElement)?.Source;
        //deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());
        deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".deputado_card--partido").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var detalhes = subDocument.QuerySelectorAll(".container>.row>.col-md-3>div>div.d-flex");

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = BuscarTexto(detalhes, "Nome Completo").ToTitleCase();

        deputado.Profissao = BuscarTexto(detalhes, "Profissão")?.ToTitleCase();
        deputado.Email = BuscarTexto(detalhes, "E-mails");
        deputado.Site = BuscarTexto(detalhes, "Site Pessoal");
        deputado.Telefone = BuscarTexto(detalhes, "Telefones");
    }

    public string BuscarTexto(IHtmlCollection<IElement> detalhes, string textoBuscar)
    {
        var elemento = detalhes.FirstOrDefault(x => x.QuerySelector("span.font-weight-bold").TextContent.Contains(textoBuscar, StringComparison.InvariantCultureIgnoreCase));
        if (elemento is not null)
        {
            return string.Join(", ", elemento.QuerySelectorAll(".text-black-50").Select(x => x.TextContent.Trim()));
        }

        return null;
    }
}
