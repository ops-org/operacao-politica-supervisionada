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
using static System.Runtime.InteropServices.JavaScript.JSType;
using static OPS.Importador.ALE.ImportadorDespesasPernambuco;

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
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();
        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");

        var dcForm = new Dictionary<string, string>();
        var tentativa = 0;
        while (true)
        {
            try
            {
                tentativa++;
                if (ano.ToString() != document.QuerySelector<IHtmlSelectElement>("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_ddlAno").Value)
                {
                    dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlAno", ano.ToString());
                    dcForm.Add("__EVENTTARGET", "dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlAno");
                    document = form.SubmitAsync(dcForm, true).GetAwaiter().GetResult();
                    form = document.QuerySelector<IHtmlFormElement>("form");
                }

                if (mes.ToString() != document.QuerySelector<IHtmlSelectElement>("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_ddlMes").Value)
                {
                    dcForm = new Dictionary<string, string>();
                    dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlMes", mes.ToString());
                    dcForm.Add("__EVENTTARGET", "dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlMes");
                    document = form.SubmitAsync(dcForm, true).GetAwaiter().GetResult();
                    form = document.QuerySelector<IHtmlFormElement>("form");
                }

                break;
            }
            catch (Exception ex)
            {
                if (tentativa >= 10)
                {
                    logger.LogError(ex.Message);
                    return;
                }

                logger.LogError(document.QuerySelector("#dnn_ctr9625_ctl00_lblMessage").TextContent);
                Thread.Sleep(TimeSpan.FromMinutes(2));
            }
        }

        // Temos de remover o elemento para recria-lo como input e poder ser submetido.
        document.QuerySelector<IHtmlInputElement>("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_btnPesquisar")?.Remove();

        var deputados = (document.QuerySelector("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_ddlGabinete") as IHtmlSelectElement);
        foreach (var deputado in deputados.Options)
        {
            tentativa = 0;
            while (true)
            {
                try
                {
                    tentativa++;
                    ConsultarDespesasDeputado(deputado, form, ano, mes);

                    break;
                }
                catch (Exception ex)
                {
                    if (tentativa >= 10)
                    {
                        logger.LogError(ex.Message);
                        throw;
                    }

                    logger.LogWarning(ex.Message);
                    Thread.Sleep(TimeSpan.FromMinutes(2));
                }
            }
        }
    }

    private void ConsultarDespesasDeputado(IHtmlOptionElement deputado, IHtmlFormElement form, int ano, int mes)
    {
        if (deputado.Value == "0") return;

        var dcForm = new Dictionary<string, string>();
        dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$ddlGabinete", deputado.Value);
        dcForm.Add("dnn$ctr9625$ViewalrsTranspRelatorioGastos$btnPesquisar", "Pesquisar");
        var subDocument = form.SubmitAsync(dcForm, true).GetAwaiter().GetResult();

        var nomeParlamentar = deputado.Text.Replace("Gabinete Dep.", "").Replace("55", "").Trim();
        var elError = subDocument.QuerySelector("#dnn_ctl01_lblMessage,#dnn_ctr9625_ctl00_lblMessage");
        if (elError != null)
        {
            throw new BusinessException($"Deputado {nomeParlamentar}; {elError.TextContent}");
        }

        var periodoPesquisa = subDocument.QuerySelector("#dnn_ctr9625_ViewalrsTranspRelatorioGastos_lblResponsavelReferencia").TextContent;
        var valorDespesasMes = subDocument.QuerySelector(".lbldespesa").TextContent.Replace("-R$ ", "");
        logger.LogInformation($"Deputado {nomeParlamentar}; Periodo: {periodoPesquisa}; Despesas: {valorDespesasMes};");

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
        logger.LogWarning("Parlamentares do(a) {idEstado}:{CasaLegislativa}", config.Estado.GetHashCode(), config.Estado.ToString());
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var address = $"{config.BaseAddress}listarDestaqueDeputados";
        DeputadosRS objDeputadosRS = RestApiGet<DeputadosRS>(address);

        foreach (var parlamentar in objDeputadosRS.Lista)
        {
            var matricula = (uint)parlamentar.IdDeputado;
            var deputado = GetDeputadoByMatriculaOrNew(matricula);

            deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
            deputado.NomeParlamentar = parlamentar.NomeDeputado.ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(parlamentar.SiglaPartido);
            deputado.Email = parlamentar.EmailDeputado;
            deputado.Telefone = parlamentar.TelefoneDeputado;
            deputado.UrlFoto = parlamentar.FotoGrandeDeputado;

            InsertOrUpdate(deputado);
        }

        logger.LogWarning("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
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
