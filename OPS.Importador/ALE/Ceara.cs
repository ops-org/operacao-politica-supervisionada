using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

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

public class ImportadorDespesasCeara : ImportadorDespesasArquivo
{
    public ImportadorDespesasCeara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.ce.gov.br/",
            Estado = Estado.Ceara,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    /// <summary>
    /// Arquivos disponiveis anualmente a partir de 2021
    /// https://transparencia.al.ce.gov.br/index.php/despesas/despesas-alece/verba-de-desempenho-parlamentar
    /// </summary>
    /// <param name="ano"></param>
    /// <returns></returns>
    public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        Dictionary<string, string> arquivos = new();

        for (int mes = 1; mes <= 12; mes++)
        {
            if (DateTime.Today.Year == ano && mes > DateTime.Today.Month) break;

            var base64 = Utils.EncodeTo64($"{mes:00}|{ano}|");
            var _urlOrigem = $"{config.BaseAddress}includes/verba_de_desempenho_parlamentar_csv.php?codigo={base64}";
            var _caminhoArquivo = $"{tempPath}/CLCE-{ano}-{mes}.csv";

            arquivos.Add(_urlOrigem, _caminhoArquivo);
        }

        return arquivos;
    }

    public override void ImportarDespesas(string caminhoArquivo, int ano)
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        int indice = 0;
        int Verba = indice++;
        int Descricao = indice++;
        int Conta = indice++;
        int Favorecido = indice++;
        int Trecho = indice++;
        int Vencimento = indice++;
        int Valor = indice++;

        var linha = 0;
        string nomeParlametar = null;
        short anoDespesa = 0;
        DateTime dataEmissao = DateTime.MinValue;
        foreach (string line in File.ReadLines(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
        {
            if (line.StartsWith("TOTAL GERAL"))
            {
                linha = 0;
                continue;
            }
            linha++;

            if (linha == 1)
            {
                nomeParlametar = CorrigeNomeParlamentar(line);
                anoDespesa = (short)ano;

                continue;
            }

            if (linha == 2)
            {
                dataEmissao = Convert.ToDateTime("01/" + line.Replace("Mes/Ano:", "").Trim(), cultureInfo);
                continue;
            }

            if (linha == 3) continue; // EMPENHO;DESCRIÇÃO;CNPJ;CREDOR;VALOR
            if (string.IsNullOrEmpty(nomeParlametar)) continue;

            var colunas = line.Split(';');
            if (colunas.Length != 6) // Finaliza com ;
                throw new Exception("Linha Invalida" + line);

            var despesaTemp = new CamaraEstadualDespesaTemp();
            despesaTemp.Nome = nomeParlametar;
            despesaTemp.Ano = anoDespesa;
            despesaTemp.DataEmissao = dataEmissao;
            despesaTemp.TipoDespesa = ObterTipoDespesa(colunas[1].Trim());
            despesaTemp.Documento = colunas[0].Trim();
            despesaTemp.Observacao = colunas[1].Trim();
            despesaTemp.CnpjCpf = colunas[2].Trim();
            despesaTemp.Empresa = colunas[3].Trim();
            despesaTemp.Valor = Convert.ToDecimal(colunas[4], cultureInfo);

            InserirDespesaTemp(despesaTemp);
        }
    }

    private string CorrigeNomeParlamentar(string nome)
    {
        nome = nome.Replace("DEP", "").Replace(".", "").Split("-")[0].Trim();

        nome = nome.ToUpper() switch
        {
            "ACRSIO SENA" => "ACRISIO SENA",
            "ANTONO GRANJA" => "ANTONIO GRANJA",
            "DANNIEL OLIVIERA" => "DANNIEL OLIVEIRA",
            "DAVID DURAN" => "DAVID DURAND",
            "DVI DE RAIMUNDAO" => "DAVI DE RAIMUNDAO",
            "GABRIELLAAGUIAR" => "GABRIELLA AGUIAR",
            "GEROGE LIMA" => "GEORGE LIMA",
            "GUILHERME BISMARK" => "GUILHERME BISMARCK",
            "GULHERME BISMARCK" => "GUILHERME BISMARCK",
            "GUUILHERME SAMPAIO" => "GUILHERME SAMPAIO",
            "JEOVA MOPTA" => "JEOVA MOTA",
            "JAO JAIME" => "JOAO JAIME",
            "JULIO CESAR" => "JULIO CESAR FILHO",
            "LUCINILDOFROTA" => "LUCINILDO FROTA",
            "MARTA GONCLAVES" => "MARTA GONCALVES",
            "ORIEL NUNES FILHO" => "ORIEL FILHO",
            "OSMKAR BAQUIT" => "OSMAR BAQUIT",
            "ROMEU ALDIGHERI" => "ROMEU ALDIGUERI",
            "´SARGENTO REGINAURO" => "SARGENTO REGINAURO",
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
