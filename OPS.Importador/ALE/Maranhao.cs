using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.ALE.Comum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE;

public class Maranhao : ImportadorBase
{
    public Maranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarMaranhao(serviceProvider);
        importadorDespesas = new ImportadorDespesasMaranhao(serviceProvider);
    }
}

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

        deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
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
        //                deputado.Matricula = (uint)i;
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

public class ImportadorParlamentarMaranhao : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.ma.leg.br/sitealema/deputados/",
            SeletorListaParlamentares = "section.section .news-card",
            Estado = Estado.Maranhao,
        });
    }


    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".news-card-title").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = deputado.NomeParlamentar;

        deputado.UrlPerfil = (item.QuerySelector(".news-card-title a") as IHtmlAnchorElement).Href;
        // deputado.UrlFoto = (item.QuerySelector("img") as IHtmlImageElement)?.Source;

        deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".news-card-chapeu").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector(".deputado img.img-full") as IHtmlImageElement)?.Source;

        var perfil = subDocument.QuerySelectorAll(".deputado>p")
            .Where(xx => xx.TextContent.Contains(":"))
            .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Replace("Ramal:", "Ramal").Split(':')[1].Trim() });

        if (!string.IsNullOrEmpty(perfil.First(x => x.Key == "Aniversário").Value))
            deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Aniversário").Value);

        deputado.Profissao = perfil.First(x => x.Key == "Profissão").Value.ToTitleCase().NullIfEmpty();
        deputado.Site = perfil.First(x => x.Key == "Site").Value.NullIfEmpty();
        deputado.Email = perfil.First(x => x.Key == "E-mail").Value.NullIfEmpty() ?? deputado.Email;
        deputado.Telefone = perfil.First(x => x.Key == "Telefone").Value;

        // ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".deputado ul a")); // Todos são as redes sociaos da AL
    }
}
