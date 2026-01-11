using System.Globalization;
using AngleSharp;
using OPS.Core.Enumerator;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Maranhao
{
    public class ImportadorDespesasMaranhao : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        private List<DeputadoEstadual> deputados = default;

        public ImportadorDespesasMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://sistemas.al.ma.leg.br/transparencia/",
                Estado = Estado.Maranhao,
                ChaveImportacao = ChaveDespesaTemp.Matricula
            };

            deputados = dbContext.DeputadosEstaduais.Where(x => x.IdEstado == config.Estado.GetHashCode()).ToList();
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {

            if (ano == 2023 && mes == 1) return;

            //for (int i = 0; i < 110; i++)
            //{
            //    //if (deputados.Any(x => x.Matricula == i)) continue;

            //    var address = $"{config.BaseAddress}lista-sintetico.html?competencia=2023-02-01&parlamentar={i}&dswid=1";
            //    var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

            //    var titulo = document.QuerySelector(".titulo-pagina").TextContent.Split("-")[2].Trim().RemoveAccents();
            //    if (!string.IsNullOrEmpty(titulo))
            //    {

            //        var deputado = deputados.FirstOrDefault(x => x.NomeParlamentar.RemoveAccents() == titulo || x.NomeCivil.RemoveAccents() == titulo); ;
            //        if (deputado != null)
            //        {
            //            if (deputado.Matricula != i)
            //            {
            //                deputado.Matricula = (int)i;
            //                connection.Update(deputado);
            //            }
            //        }
            //        else
            //        {
            //            var valorGasto = document.QuerySelector(".ui-datatable-footer .p-text-bold").TextContent.Split(":").Last().Trim();
            //            Console.WriteLine($"{i};{titulo};{valorGasto}");
            //        }
            //    }
            //}

            var corteMatricula = (deputados.Max(x => x.Matricula) + 10);
            for (int matricula = 0; matricula < corteMatricula; matricula++)
            {
                var address = $"{config.BaseAddress}lista-sintetico.html?competencia={ano}-{mes:00}-01&parlamentar={matricula}&dswid=1";
                var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

                var nomeParlamentar = document.QuerySelector(".titulo-pagina").TextContent.Split("-")[2].Trim();
                var competencia = Convert.ToDateTime(document.QuerySelector(".ui-datatable-header.ui-corner-top").TextContent.Split(':')[1]);
                var linhasDespesas = document.QuerySelectorAll(".ui-datatable-tablewrapper tbody tr");

                foreach (var linha in linhasDespesas)
                {
                    var colunas = linha.QuerySelectorAll("td");
                    if (colunas[0].TextContent == "Nenhum registro encontrado.") break;

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = nomeParlamentar,
                        Cpf = matricula.ToString(),
                        Ano = (short)ano,
                        Mes = (short)mes,
                        DataEmissao = competencia
                    };

                    despesaTemp.TipoDespesa = colunas[1].TextContent.Trim();
                    despesaTemp.Valor = Convert.ToDecimal(colunas[2].TextContent.Replace("R$ ", ""), cultureInfo);

                    if (despesaTemp.Valor > 0)
                        InserirDespesaTemp(despesaTemp);
                }
            }
        }
    }
}
