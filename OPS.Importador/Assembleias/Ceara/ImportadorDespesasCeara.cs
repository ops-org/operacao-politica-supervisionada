using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Ceara;

public class ImportadorDespesasCeara : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasCeara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.ce.gov.br/index.php/despesas/despesas-alece/verba-de-desempenho-parlamentar",
            Estado = Estados.Ceara,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override async Task ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var document = await context.OpenAsyncAutoRetry(config.BaseAddress);

        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");

        var dcForm = new Dictionary<string, string>();
        dcForm.Add("opcao", "1");
        dcForm.Add("mes", mes.ToString("00"));
        dcForm.Add("ano", ano.ToString());
        dcForm.Add("nome", "");
        document = await form.SubmitAsyncAutoRetry(dcForm, true);

        var deputados = document.QuerySelectorAll("table#table tbody tr");
        foreach (var deputado in deputados)
        {
            var colunas = deputado.QuerySelectorAll("td");
            var nomeParlamentar = colunas.First().TextContent.Trim();
            if (string.IsNullOrEmpty(nomeParlamentar)) continue;

            var linkDetalhes = colunas.ElementAt(1).QuerySelector("a").Attributes["onclick"].Text();

            int srcIndex = linkDetalhes.IndexOf(".src='");
            if (srcIndex != -1)
            {
                int startIndex = srcIndex + 6; // Length of ".src='"
                int endIndex = linkDetalhes.IndexOf("'", startIndex);

                linkDetalhes = linkDetalhes.Substring(startIndex, endIndex - startIndex);
            }

            var documentDetalhes = await context.OpenAsyncAutoRetry(linkDetalhes);
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
                    DataEmissao = new DateOnly(ano, mes, 1),
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

        nome = nome.ToUpper() switch
        {
            "PLANO DE SAUDE" => string.Empty,
            "225/03" => string.Empty, // TODO: Arquivo 2023-07
            _ => nome
        };

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
