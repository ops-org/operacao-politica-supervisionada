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
            ChaveImportacao = ChaveDespesaTemp.Nome
        };
    }

    /// <summary>
    /// Arquivos disponiveis anualmente a partir de 2021
    /// https://transparencia.al.ce.gov.br/index.php/despesas/verba-de-desempenho-parlamentar
    /// </summary>
    /// <param name="ano"></param>
    /// <returns></returns>
    public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        Dictionary<string, string> arquivos = new();

        for (int mes = 1; mes <= 12; mes++)
        {
            if (DateTime.Today.Year == ano && DateTime.Today.Month > mes) break;

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
        var despesaTemp = new CamaraEstadualDespesaTemp();
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
                despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = line.Replace("DEP", "").Trim(),
                    Ano = (short)ano
                };

                continue;
            }

            if (linha == 2)
            {
                despesaTemp.DataEmissao = Convert.ToDateTime("01/" + line.Replace("Mes/Ano:", "").Trim(), cultureInfo);
                continue;
            }

            if (linha == 3) // EMPENHO;DESCRIÇÃO;CNPJ;CREDOR;VALOR
                continue;

            var colunas = line.Split(';');
            if (colunas.Length != 6) // Finaliza com ;
                throw new Exception("Linha Invalida" + line);

            despesaTemp.Id = 0;
            despesaTemp.TipoDespesa = ObterTipoDespesa(colunas[1].Trim());
            despesaTemp.Documento = colunas[0].Trim();
            despesaTemp.Observacao = colunas[1].Trim();
            despesaTemp.CnpjCpf = colunas[2].Trim();
            despesaTemp.Empresa = colunas[3].Trim();
            despesaTemp.Valor = Convert.ToDecimal(colunas[4], cultureInfo);

            InserirDespesaTemp(despesaTemp);
        }
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
            case "SERVIÇOS POSTAIS": return "Serviços POstais";
            case "TELEFONIA": return "Telefonia";
            case "TV": return "Internet e TV";
        }

        return null;
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
        var nomeparlamentar = parlamentar.QuerySelector(".deputado_card--nome").TextContent.Trim();
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

        deputado.NomeCivil = BuscarTexto(detalhes, "Nome Completo");
        deputado.Profissao = BuscarTexto(detalhes, "Profissão");
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
