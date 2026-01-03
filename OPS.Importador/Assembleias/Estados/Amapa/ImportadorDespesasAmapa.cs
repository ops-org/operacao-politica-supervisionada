using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Amapa
{
    public class ImportadorDespesasAmapa : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        private readonly List<DeputadoEstadual> deputados;

        public ImportadorDespesasAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.ap.gov.br/",
                Estado = Estado.Amapa,
                ChaveImportacao = ChaveDespesaTemp.Matricula
            };

            // TODO: Filtrar legislatura atual
            deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {
            var address = $"{config.BaseAddress}transparencia/gabinete_ceap_json.php?ano_verbaB={ano}&mes_verbaB={mes:00}";
            var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
            var gabinetes = document.QuerySelectorAll("option").ToList();

            foreach (var item in gabinetes)
            {
                var gabinete = item as IHtmlOptionElement;
                if (string.IsNullOrEmpty(gabinete.Value)) continue;

                var deputado = deputados.Find(x => gabinete.Value.Contains(x.Gabinete.ToString()));
                if (deputado == null)
                {
                    logger.LogError("Parlamentar {Gabinete}: {Parlamentar} não existe ou não possui gabinete relacionado!", gabinete.Value, gabinete.Text);
                }

                address = $"{config.BaseAddress}transparencia/pagina.php?pg=ceap&acao=buscar&ano_verbaB={ano}&mes_verbaB={mes:00}&idgabineteB={gabinete.Value}";
                document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

                if (document.QuerySelector(".ls-alert-warning")?.TextContent == "Nenhum: resultado foi encontrado!") continue;

                var tabelaDespesas = document.QuerySelector(".ls-table");
                var linhasDespesas = tabelaDespesas.QuerySelectorAll("tbody tr");

                foreach (var linha in linhasDespesas)
                {
                    var primeiraColuna = linha.QuerySelectorAll("td")[0];
                    if (primeiraColuna.TextContent == "TOTAL") continue;

                    var linkDetalhes = (primeiraColuna.QuerySelector("a") as IHtmlAnchorElement);


                    var subDocument = context.OpenAsyncAutoRetry(linkDetalhes.Href).GetAwaiter().GetResult();
                    var linhasDespesasDetalhes = subDocument.QuerySelectorAll(".ls-table tbody tr");
                    foreach (var detalhes in linhasDespesasDetalhes)
                    {
                        var colunas = detalhes.QuerySelectorAll("td");
                        if (colunas[0].TextContent == "TOTAL") continue;

                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = gabinete.Text.Split("-")[0].Trim().ToTitleCase(),
                            Cpf = deputado?.Matricula?.ToString(),
                            Ano = (short)ano,
                            Mes = (short)mes,
                            TipoDespesa = linkDetalhes.Text.Split(" - ")[1].Trim(),
                        };

                        var empresaParts = colunas[0].TextContent.Split(" - ");
                        despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(empresaParts[0].Trim());
                        despesaTemp.Empresa = empresaParts[1].Trim();

                        despesaTemp.Documento = colunas[2].TextContent.Trim();
                        despesaTemp.Observacao = (colunas[2].QuerySelector("a") as IHtmlAnchorElement).Href;
                        despesaTemp.Valor = Convert.ToDecimal(colunas[3].TextContent, cultureInfo);
                        despesaTemp.DataEmissao = new DateTime(ano, mes, 1);

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }
        }
    }
}
