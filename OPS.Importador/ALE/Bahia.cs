using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using Serilog;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado da Bahia
/// 
/// </summary>
public class Bahia : ImportadorBase
{
    public Bahia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarBahia(serviceProvider);
        importadorDespesas = new ImportadorDespesasBahia(serviceProvider);
    }
}

public class ImportadorDespesasBahia : ImportadorDespesasRestApiAnual
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasBahia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.al.ba.gov.br/",
            Estado = Estado.Bahia,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        var pagina = 0;
        var despesasReferencia = new List<string>();
        while (true)
        {
            logger.LogTrace("Consultando pagina {Pagina}!", pagina);
            // Pagina começa em 0 - HUE
            var address = $"{config.BaseAddress}transparencia/verbas-idenizatorias?ano={ano}&categoria=&page={pagina++}&size=1000";
            var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
            var despesas = document.QuerySelectorAll(".tabela-cab tbody tr");
            foreach (var despesa in despesas)
            {
                // N° PROCESSO	N° NF	MÊS/ANO	DEPUTADO (A)	CATEGORIA	VALOR (R$)
                var colunas = despesa.QuerySelectorAll("td");
                if (!colunas.Any()) continue;

                var processo = colunas[0].QuerySelector("span").TextContent.Trim();
                if (despesasReferencia.Contains(processo)) continue;
                despesasReferencia.Add(processo);

                var linkDetalhes = (colunas[0].Children[0] as IHtmlAnchorElement).Href;
                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Ano = (short)ano,
                    //Documento = colunas[0].QuerySelector("span").TextContent.Trim() + "/" + colunas[1].TextContent.Trim(),
                    DataEmissao = Convert.ToDateTime("01/" + colunas[2].TextContent.Trim(), cultureInfo),
                    Nome = colunas[3].TextContent.Trim().ToTitleCase(),
                    //TipoDespesa = colunas[4].TextContent.Trim()
                };

                try
                {
                    var documentDetalhes = context.OpenAsyncAutoRetry(linkDetalhes).GetAwaiter().GetResult();
                    var despesasDetalhes = documentDetalhes.QuerySelectorAll(".tabela-cab tbody tr");
                    foreach (var detalhes in despesasDetalhes)
                    {
                        // CATEGORIA	Nº NOTA/RECIBO	CPF/CNPJ	NOME DO FORNECEDOR	VALOR ANEXO NF
                        var colunasDetalhes = detalhes.QuerySelectorAll("td");

                        despesaTemp.TipoDespesa = colunasDetalhes[0].TextContent.Trim();
                        despesaTemp.Documento = processo + "/" + colunasDetalhes[1].TextContent.Trim();
                        despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(colunasDetalhes[2].TextContent.Trim());
                        despesaTemp.Empresa = colunasDetalhes[3].TextContent.Trim();
                        despesaTemp.Valor = Convert.ToDecimal(colunasDetalhes[4].TextContent.Replace("R$", "").Trim(), cultureInfo);

                        if (colunasDetalhes[5].HasChildNodes)
                            despesaTemp.Observacao = (colunasDetalhes[5].Children[0] as IHtmlAnchorElement).Href;
                    }
                }
                catch (Exception ex)
                {
                    // TODO? Revisar
                    using (var reader = connection.ExecuteReader($"SELECT distinct hash FROM cl_despesa WHERE numero_documento LIKE '{processo}/%'"))
                    {
                        while (reader.Read())
                        {
                            var key = Convert.ToHexString((byte[])reader["hash"]);
                            lstHash.Remove(key);
                        }
                    }

                    Log.Error(ex, ex.Message);
                    continue;
                }

                InserirDespesaTemp(despesaTemp);
            }

            if (document.QuerySelector(".paginate-button-next")?.ClassList.Contains("disabled") ?? true) break;
        }
    }
}

public class ImportadorParlamentarBahia : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarBahia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // https://www.al.ba.gov.br/deputados/deputados-estaduais (Em Exercicio)
        // https://www.al.ba.gov.br/deputados/legislatura-atual

        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.ba.gov.br/deputados/legislatura-atual",
            SeletorListaParlamentares = ".fe-div-table>div",
            Estado = Estado.Bahia,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var nomeparlamentar = parlamentar.QuerySelector(".deputado-nome span").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (parlamentar.QuerySelector(".list-item a") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (parlamentar.QuerySelector(".list-item img") as IHtmlImageElement)?.Source;
        deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"/").Last());
        deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".partido-nome").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var detalhes = subDocument.QuerySelectorAll(".dados-deputado p");
        deputado.NomeCivil = detalhes[0].QuerySelector("span").TextContent.Trim().ToTitleCase();

        var profissaoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("PROFISSÃO", StringComparison.InvariantCultureIgnoreCase));
        if (profissaoElemento is not null)
        {
            deputado.Profissao = profissaoElemento.QuerySelector("span").TextContent.Trim().ToTitleCase();
        }

        var nascimentoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("NASCIMENTO", StringComparison.InvariantCultureIgnoreCase));
        if (nascimentoElemento is not null)
        {
            var nascimentoComNaturalidade = nascimentoElemento.QuerySelector("span").TextContent.Split(',');
            deputado.Nascimento = DateOnly.Parse(nascimentoComNaturalidade[0].Trim(), cultureInfo);
            deputado.Naturalidade = nascimentoComNaturalidade[1].Trim();
        }

        var sexoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("SEXO", StringComparison.InvariantCultureIgnoreCase));
        if (sexoElemento is not null)
            deputado.Sexo = sexoElemento.QuerySelector("span").TextContent.Trim()[0].ToString();

        var contatos = subDocument.QuerySelectorAll(".fe-dep-dados-ajsut-mobile .linha-cv strong").First(x => x.TextContent.Trim() == "Contato").ParentElement;
        var telefoneElemento = contatos.QuerySelectorAll("p").FirstOrDefault(x => x.TextContent.StartsWith("Tel"));
        if (telefoneElemento != null)
            deputado.Telefone = string.Join(" ", telefoneElemento.ParentElement.QuerySelectorAll("span").Select(x => x.TextContent.Trim()));

        deputado.Email = contatos.QuerySelector("p a span").TextContent?.Trim().NullIfEmpty();
    }
}
