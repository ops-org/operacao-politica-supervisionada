using System.Globalization;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerator;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.EspiritoSanto
{
    public class ImportadorDespesasEspiritoSanto : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        private readonly List<DeputadoEstadual> deputados;

        public ImportadorDespesasEspiritoSanto(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.es.gov.br/",
                Estado = Estado.EspiritoSanto,
                ChaveImportacao = ChaveDespesaTemp.Gabinete
            };

            // TODO: Filtrar legislatura atual
            deputados = dbContext.DeputadosEstaduais.Where(x => x.IdEstado == config.Estado.GetHashCode()).ToList();
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {
            var address = $"{config.BaseAddress}Transparencia/CotasParlamentares";
            var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
            var gabinetes = document.QuerySelectorAll("#cboSetor option").ToList();

            foreach (var item in gabinetes)
            {
                var gabinete = item as IHtmlOptionElement;
                if (gabinete.Value == "0") continue;

                var deputado = deputados.Find(x => x.Gabinete.ToString() == gabinete.Value);
                if (deputado == null)
                {
                    deputado = deputados.Find(x => gabinete.Text.Replace("Gab. Dep. ", "").Contains(x.NomeParlamentar.ToString()));

                    if (deputado != null)
                    {
                        deputado.Gabinete = Convert.ToInt32(gabinete.Value);
                        dbContext.SaveChanges();
                    }
                    else
                        logger.LogError("Parlamentar {Gabinete}: {Parlamentar} não existe ou não possui gabinete relacionado!", gabinete.Value, gabinete.Text);
                }

                address = $"{config.BaseAddress}/Transparencia/Api/CotasParlamentaresNovoTable/{mes}/{mes}/{ano}/{gabinete.Value}/false";
                document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

                if (document.QuerySelector(".ls-alert-warning")?.TextContent == "Nenhum: resultado foi encontrado!") continue;

                var rows = document.QuerySelectorAll("#tabelaMobile table tr");
                for (var i = 0; i < rows.Count() - 1; i++)
                {
                    if (rows[i].QuerySelectorAll("td")[0].TextContent.Trim() != "Produto:") continue;
                    if (rows[i + 2].QuerySelectorAll("td")[1].TextContent.Trim() == "-") continue;

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = gabinete.Text.Replace("Gab. Dep. ", ""),
                        Cpf = gabinete.Value,
                        Ano = (short)ano,
                        Mes = (short)mes,
                        DataEmissao = new DateTime(ano, mes, 1),
                        Valor = Convert.ToDecimal(rows[i + 2].QuerySelectorAll("td")[1].TextContent.Replace("R$ ", "").Trim(), cultureInfo),
                        Observacao = "Quantidade: " + rows[i + 1].QuerySelectorAll("td")[1].TextContent.Trim(),
                        TipoDespesa = rows[i].QuerySelectorAll("td")[1].TextContent.Trim(),
                    };

                    if (despesaTemp.Valor > 0)
                        InserirDespesaTemp(despesaTemp);
                }
            }
        }
    }
}
