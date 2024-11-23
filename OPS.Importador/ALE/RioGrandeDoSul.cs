using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
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

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado de Rio Grande do Sul
/// </summary>
public class RioGrandeDoSul : ImportadorBase
{
    public RioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarRioGrandeDoSul(serviceProvider);
        importadorDespesas = new ImportadorDespesasRioGrandeDoSul(serviceProvider);
    }
}

public class ImportadorDespesasRioGrandeDoSul : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www2.al.rs.gov.br/transparenciaalrs/GabinetesParlamentares/Centrodecustos/tabid/5666/Default.aspx",
            Estado = Estado.RioGrandeDoSul,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar // O Gabinete muda a cada legislatura
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();
        var mensagem = document.QuerySelector(".style_titulo");
        if(mensagem != null)
        {
            logger.LogError(mensagem.TextContent.Trim());
            throw new BusinessException(mensagem.TextContent.Trim());
        }

        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");

        var dcForm = new Dictionary<string, string>();
        if (ano.ToString() != document.QuerySelector<IHtmlSelectElement>("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_ddlAno").Value)
        {
            dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlAno", ano.ToString());
            dcForm.Add("__EVENTTARGET", "dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlAno");
            document = SubmitWithAutoRetryAsync(form, dcForm, ano, mes, string.Empty);
            form = document.QuerySelector<IHtmlFormElement>("form");
        }

        if (mes.ToString() != document.QuerySelector<IHtmlSelectElement>("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_ddlMes").Value)
        {
            dcForm = new Dictionary<string, string>();
            dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlMes", mes.ToString());
            dcForm.Add("__EVENTTARGET", "dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlMes");
            document = SubmitWithAutoRetryAsync(form, dcForm, ano, mes, string.Empty);
            form = document.QuerySelector<IHtmlFormElement>("form");
        }

        // Temos de remover o elemento para recria-lo como input e poder ser submetido.
        document.QuerySelector<IHtmlInputElement>("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_btnPesquisar")?.Remove();

        var deputados = (document.QuerySelector("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_ddlGabinete") as IHtmlSelectElement);
        foreach (var deputado in deputados.Options)
        {
            ConsultarDespesasDeputado(deputado, form, ano, mes);
        }
    }

    private void ConsultarDespesasDeputado(IHtmlOptionElement deputado, IHtmlFormElement form, int ano, int mes)
    {
        if (deputado.Value == "0") return;

        var nomeParlamentar = deputado.Text.Replace("Gabinete Dep.", "").Replace("55", "").Trim();

        var dcForm = new Dictionary<string, string>();
        dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlGabinete", deputado.Value);
        dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$btnPesquisar", "Pesquisar");
        IDocument subDocument = SubmitWithAutoRetryAsync(form, dcForm, ano, mes, nomeParlamentar);

        var periodoPesquisa = subDocument.QuerySelector("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_lblResponsavelReferencia").TextContent;
        var valorDespesasMes = subDocument.QuerySelector(".lbldespesa").TextContent.Replace("-R$ ", "");
        //logger.LogInformation($"Deputado {nomeParlamentar}; Periodo: {periodoPesquisa}; Despesas: {valorDespesasMes};");

        var despesas = subDocument.QuerySelectorAll<IHtmlTableRowElement>(".grdvwgastosinterno tr");
        foreach (var item in despesas)
        {
            if (item.Cells[1].TextContent == "Valor") continue;

            var despesaTemp = new CamaraEstadualDespesaTemp()
            {
                Nome = nomeParlamentar,
                Cpf = deputado.Value,
                Ano = (short)ano,
                Mes = (short)mes,
                TipoDespesa = item.Cells[0].TextContent,
                Valor = Math.Abs(Convert.ToDecimal(item.Cells[1].TextContent.Replace("R$ ", ""), cultureInfo)),
                DataEmissao = new DateTime(ano, mes, 1)
            };

            if (despesaTemp.Valor == 0) continue;

            InserirDespesaTemp(despesaTemp);
        }
    }

    private IDocument SubmitWithAutoRetryAsync(IHtmlFormElement form, Dictionary<string, string> dcForm, int ano, int mes, string nomeParlamentar)
    {
        var document = form.SubmitAsyncAutoRetry(dcForm, true).GetAwaiter().GetResult();

        for (int i = 0; i < 10; i++)
        {
            var elError = document.QuerySelector("#dnn_ctl01_lblMessage,#dnn_ctr9625_ctl00_lblMessage");
            if (elError != null)
            {
                if (elError.TextContent.Contains("Timeout expired", StringComparison.CurrentCultureIgnoreCase) || elError.TextContent.Contains("currently unavailable", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(nomeParlamentar))
                        logger.LogWarning("Erro ao consulta: {mes:00}/{ano}; Nova tentativa em 2min.", mes, ano);
                    else
                        logger.LogWarning("Erro ao consulta Parlamentar {nomeParlamentar}; Referencia: {mes:00}/{ano}; Nova tentativa em 2min.", nomeParlamentar, mes, ano);

                    Thread.Sleep(TimeSpan.FromMinutes(2));

                    document = form.SubmitAsyncAutoRetry(dcForm, true).GetAwaiter().GetResult();
                    continue;
                }

                throw new BusinessException($"Erro ao consulta Parlamentar {nomeParlamentar}; Referencia: {mes:00}/{ano}; {elError.TextContent}");
            }

            break;
        }

        return document;
    }
}

public class ImportadorParlamentarRioGrandeDoSul : ImportadorParlamentarRestApi
{

    public ImportadorParlamentarRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "https://ww4.al.rs.gov.br:5000/",
            Estado = Estado.RioGrandeDoSul,
        });
    }

    public override Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var address = $"{config.BaseAddress}listarDestaqueDeputados";
        DeputadosRS objDeputadosRS = RestApiGet<DeputadosRS>(address);

        foreach (var parlamentar in objDeputadosRS.Lista)
        {
            var matricula = (uint)parlamentar.IdDeputado;
            var deputado = GetDeputadoByMatriculaOrNew(matricula);

            deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
            deputado.NomeParlamentar = parlamentar.NomeDeputado.Trim().ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(parlamentar.SiglaPartido);
            deputado.Email = parlamentar.EmailDeputado;
            deputado.Telefone = parlamentar.TelefoneDeputado;
            deputado.UrlFoto = parlamentar.FotoGrandeDeputado;

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = deputado.NomeParlamentar;

            InsertOrUpdate(deputado);
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        return Task.CompletedTask;
    }
}


public class DeputadoRS
{
    [JsonPropertyName("codigoPro")]
    public int CodigoPro { get; set; }

    [JsonPropertyName("idDeputado")]
    public int IdDeputado { get; set; }

    [JsonPropertyName("nomeDeputado")]
    public string NomeDeputado { get; set; }

    [JsonPropertyName("emailDeputado")]
    public string EmailDeputado { get; set; }

    [JsonPropertyName("telefoneDeputado")]
    public string TelefoneDeputado { get; set; }

    [JsonPropertyName("siglaPartido")]
    public string SiglaPartido { get; set; }

    [JsonPropertyName("nomePartido")]
    public string NomePartido { get; set; }

    [JsonPropertyName("codStatus")]
    public int CodStatus { get; set; }

    [JsonPropertyName("fotoGrandeDeputado")]
    public string FotoGrandeDeputado { get; set; }
}

public class DeputadosRS
{
    [JsonPropertyName("lista")]
    public List<DeputadoRS> Lista { get; set; }
}
