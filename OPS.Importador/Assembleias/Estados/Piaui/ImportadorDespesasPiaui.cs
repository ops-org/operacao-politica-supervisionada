using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;

namespace OPS.Importador.Assembleias.Estados.Piaui
{
    public class ImportadorDespesasPiaui : ImportadorDespesasArquivo
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasPiaui(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://transparencia.al.pi.leg.br/",
                Estado = Estado.Piaui,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }

        public override void Importar(int ano)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                // TODO: Criar importação por legislatura
                if (ano != 2023)
                {
                    logger.LogWarning("Importação já realizada para o ano de {Ano}", ano);
                    throw new BusinessException($"Importação já realizada para o ano de {ano}");
                }

                CarregarHashes(ano);

                var caminhoArquivo = $"{tempPath}/grid_transp_publico_gecop.csv";

                var fileInfo = new FileInfo(caminhoArquivo);
                if (fileInfo.CreationTime > DateTime.Now.AddMonths(-1))
                {
                    logger.LogError("Arquivo de importação criado em {FileCreationTime} pode estar desatualizado!", fileInfo.CreationTime);
                }

                using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("UTF-8")))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.CreateSpecificCulture("pt-BR")))
                    {
                        while (csv.Read())
                        {
                            var despesaTemp = new CamaraEstadualDespesaTemp();
                            despesaTemp.DataEmissao = Convert.ToDateTime("01/" + csv[ColunasDespesas.Competencia.GetHashCode()], cultureInfo);
                            despesaTemp.Ano = (short)despesaTemp.DataEmissao.Year;
                            despesaTemp.Mes = (short)despesaTemp.DataEmissao.Month;
                            despesaTemp.Nome = csv[ColunasDespesas.Parlamentar.GetHashCode()].Trim().ToTitleCase();
                            despesaTemp.TipoDespesa = csv[ColunasDespesas.Subcota.GetHashCode()].Trim();
                            despesaTemp.Valor = Convert.ToDecimal(csv[ColunasDespesas.Valor.GetHashCode()], cultureInfo);

                            InserirDespesaTemp(despesaTemp);
                        }
                    }
                }

                ProcessarDespesas(ano);
            }
        }

        public override void DefinirCompetencias(int ano)
        {
            competenciaInicial = $"{ano}01";
            competenciaFinal = $"{ano + 4}12";
        }

        public override void ImportarDespesas(string caminhoArquivo, int ano)
        {
            throw new NotImplementedException();
        }

        private enum ColunasDespesas
        {
            Legislatura,
            Parlamentar,
            Competencia,
            Subcota,
            Valor
        }

        //public override void ImportarDespesas(IBrowsingContext context, int ano)
        //{
        //    var configuration = AngleSharp.Configuration.Default
        //            .With(new HttpClientRequester(httpClient))
        //            .WithDefaultLoader(new LoaderOptions { IsResourceLoadingEnabled = true })
        //            .WithJs()
        //            .WithDefaultCookies()
        //            .WithCulture("pt-BR");

        //    context = BrowsingContext.New(configuration);


        //    var address = $"{config.BaseAddress}grid_transp_publico_gecop/";
        //    var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

        //    //var filtroLegislaturaAtual = (document.QuerySelectorAll(".simplemv_legis a.scGridRefinedSearchCampoFont").First(x => x.TextContent.Trim() == "20a Legislatura (2023 a 2027)") as IHtmlAnchorElement);
        //    //filtroLegislaturaAtual.DoClick();

        //    while (true)
        //    {
        //        var pagina = document.QuerySelector(".scGridToolbarNavOpen").TextContent;
        //        Console.WriteLine($"Consultando pagina {pagina}");

        //        var rows = document.QuerySelectorAll("#sc_grid_body>table>tbody");
        //        var despesaTemp = new CamaraEstadualDespesaTemp();
        //        foreach (var row in rows)
        //        {
        //            if (row.Id.EndsWith("_bot"))
        //            {
        //                var despesas = row.QuerySelectorAll("tr");
        //                foreach (var despesa in despesas)
        //                {
        //                    var colunas = despesa.QuerySelectorAll(">td");
        //                    if (colunas.Length != 8) continue; // Ignorar linha de detalhamento;
        //                    var parlamentar = colunas[ColunasDespesas.Parlamentar.GetHashCode()].TextContent.Trim();
        //                    if (parlamentar == "Competência" || colunas.Any(x=> x.TextContent.Trim() == "Docto")) continue;

        //                    despesaTemp.DataEmissao = Convert.ToDateTime("01/" + colunas[ColunasDespesas.Competencia.GetHashCode()].TextContent, cultureInfo);
        //                    despesaTemp.Ano = (short)ano;
        //                    despesaTemp.Mes = (short)despesaTemp.DataEmissao.Month;
        //                    despesaTemp.Nome = parlamentar.Trim().ToTitleCase();
        //                    despesaTemp.TipoDespesa = colunas[ColunasDespesas.Subcota.GetHashCode()].TextContent;
        //                    despesaTemp.Valor = Convert.ToDecimal(colunas[ColunasDespesas.Valor.GetHashCode()].TextContent, cultureInfo);

        //                    InserirDespesaTemp(despesaTemp);
        //                }
        //            }
        //            else
        //            {
        //                Console.WriteLine(row.QuerySelector(".scGridBlockFont td").TextContent);
        //                //throw new BusinessException("Linha invalida!");
        //            }
        //        }

        //        var proximaPagina = document.QuerySelector("#forward_bot");
        //        if (proximaPagina.ClassList.Contains("disabled")) break;

        //        (proximaPagina as IHtmlAnchorElement).DoClick();
        //    }
        //}

        //private enum ColunasDespesas
        //{
        //    Legislatura = 3,
        //    Parlamentar,
        //    Competencia,
        //    Subcota,
        //    Valor
        //}

    }
}
