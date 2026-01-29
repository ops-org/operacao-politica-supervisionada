using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Amazonas
{
    public class ImportadorDespesasAmazonas : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasAmazonas(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.aleam.gov.br/transparencia/controle-de-cota-parlamentar/",
                Estado = Estados.Amazonas,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {
            // Pagina usa caixas de seleção escondidas, trocadas a cada legislatura.
            var chaveDeputados = ano == 2023 && mes == 1 ? "dadosatual" : "dados";

            var document = await context.OpenAsyncAutoRetry(config.BaseAddress);
            var deputados = (document.QuerySelector($"#{chaveDeputados}") as IHtmlSelectElement);

            IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form");

            foreach (var deputado in deputados.Options)
            {
                using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = deputado.Text }))
                {
                    var dcForm = new Dictionary<string, string>();
                    dcForm.Add("ano", ano.ToString());
                    dcForm.Add("mes", mes.ToString("00"));
                    dcForm.Add("dados", deputado.Value);
                    dcForm.Add("dadosatual", deputado.Value);
                    var subDocument = await form.SubmitAsyncAutoRetry(dcForm);

                    if (subDocument.QuerySelector(".no-events") != null || subDocument.QuerySelector(".no-events")?.TextContent == "Não há resultados para a sua pesquisa.") continue;

                    var despesas = subDocument.QuerySelectorAll(".table-dados .modal");

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

                        // TODO: Verificar que bruxaria acontece para trazer dados misturados. Não ocorre no navegador.
                        if (deputado.Text != "Abdala Fraxe" && dc["DEPUTADO"].ToTitleCase() == "Abdala Habib Fraxe Junior") continue;
                        //logger.LogInformation($"Consultando Parlamentar {deputado.Value}: {deputado.Text} - {dc["DEPUTADO"].ToTitleCase()}");

                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = deputado.Text, // Nome Parlamentar
                            Cpf = deputado.Value,
                            Ano = (short)ano,
                            Mes = (short)mes,
                            Favorecido = dc["DEPUTADO"].ToTitleCase(), // Nome Civil
                            TipoDespesa = dc["DESCRIÇÃO DA VERBA"].ToTitleCase(),
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(dc["CNPJ"]),
                            NomeFornecedor = dc["NOME EMPRESARIAL"].ToTitleCase(),
                            Documento = dc["IDENTIFICAÇÃO DO DOCUMENTO"],
                            Valor = Convert.ToDecimal(dc["VALOR LÍQUIDO"].Replace("R$ ", ""), cultureInfo),
                            DataEmissao = DateOnly.Parse(dc["EMISSÃO"], cultureInfo)
                        };

                        var valorGlosa = dc["VALOR DA GLOSA"];
                        if (valorGlosa != "R$ 0,00")
                        {
                            despesaTemp.Observacao = $"Valor Bruto: {dc["VALOR BRUTO"]}; Valor da Glosa: {valorGlosa};";
                        }

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }
        }
    }
}
