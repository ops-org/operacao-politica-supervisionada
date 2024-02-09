using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using Serilog.Events;

namespace OPS.Importador.ALE;

public class Amazonas : ImportadorBase
{
    public Amazonas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // importadorParlamentar = new ImportadorParlamentarAmazonas(serviceProvider);
        importadorDespesas = new ImportadorDespesasAmazonas(serviceProvider);
    }
}

public class ImportadorDespesasAmazonas : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasAmazonas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.aleam.gov.br/transparencia/controle-de-cota-parlamentar/",
            Estado = Estado.Amazonas,
            ChaveImportacao = ChaveDespesaTemp.Nome
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();
        var deputados = (document.QuerySelector("#dados") as IHtmlSelectElement);

        foreach (var deputado in deputados.Options)
        {
            IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form#pesq");

            var dcForm = new Dictionary<string, string>();
            dcForm.Add("ano", ano.ToString());
            dcForm.Add("mes", mes.ToString("00"));
            dcForm.Add("dadosatual", deputado.Value);
            document = form.SubmitAsync(dcForm, true).GetAwaiter().GetResult();

            var despesas = document.QuerySelectorAll(".table-dados .modal");
            foreach (var item in despesas)
            {
                var dc = new Dictionary<string, string>();
                var registros = item.QuerySelectorAll(".box-body");
                foreach (var registro in registros)
                {
                    if (string.IsNullOrEmpty(registro.TextContent.Trim())) continue;

                    var key = registro.QuerySelector(".ceap-sub-titulo").TextContent.Trim();
                    var value = registro.QuerySelector(".ceap-titulo,.ceap-titulo-email").TextContent.Trim();

                    dc.Add(key, value);
                }

                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = deputado.Text, // Nome Parlamentar
                    Cpf = deputado.Value,
                    Ano = (short)ano,
                    Mes =  (short)mes,
                    Favorecido= dc["DEPUTADO"].ToTitleCase(), // Nome Civil
                    TipoDespesa = dc["DESCRIÇÃO DA VERBA"].ToTitleCase(),
                    CnpjCpf = Utils.RemoveCaracteresNaoNumericos(dc["CNPJ"]),
                    Empresa = dc["NOME EMPRESARIAL"].ToTitleCase(),
                    Documento = dc["IDENTIFICAÇÃO DO DOCUMENTO"],
                    Valor = Convert.ToDecimal(dc["VALOR LÍQUIDO"].Replace("R$ ", ""), cultureInfo),
                    DataEmissao = Convert.ToDateTime(dc["EMISSÃO"], cultureInfo)
                };

                var valorGlosa = dc["VALOR DA GLOSA"];
                if (valorGlosa != "R$ 0,00")
                {
                    despesaTemp.Observacao = $"Valor Bruto: {dc["VALOR BRUTO"]}; Valor da Glosa: {valorGlosa};";
                }

                InserirDespesaTemp(despesaTemp);
            }
        }

        //Thread.Sleep(TimeSpan.FromSeconds(3));
    }
}

public class ImportadorParlamentarAmazonas : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarAmazonas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.aleam.gov.br/deputados/",
            SeletorListaParlamentares = ".dep .dep-int-cont",
            Estado = Estado.Amazonas,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        var nomeparlamentar = document.QuerySelector(".dep-int-cont__title").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlFoto = (document.QuerySelector("img") as IHtmlImageElement)?.Source;
        deputado.UrlPerfil = (document.QuerySelector("a") as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(document.QuerySelector(".dep-int-cont__part").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.NomeCivil = subDocument.QuerySelector("#nome-dep").TextContent.Trim();
        deputado.Naturalidade = subDocument.QuerySelector("#nat-dep").TextContent.Trim();
        if (subDocument.QuerySelector("#ani-dep") != null)
            deputado.Nascimento = DateOnly.Parse(subDocument.QuerySelector("#ani-dep").TextContent.Trim(), cultureInfo);
        deputado.Email = subDocument.QuerySelector("#email-dep").TextContent.Trim();
        deputado.Telefone = subDocument.QuerySelector("#tel-dep").TextContent.Trim().Replace("NA", "").NullIfEmpty();
        deputado.Sexo = deputado.Email.StartsWith("deputada.") ? "F" : "M";

        ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll("#redes-pf-dep a"));
    }
}
