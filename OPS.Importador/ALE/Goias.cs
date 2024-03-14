using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using RestSharp;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado de Goias
/// https://portal.al.go.leg.br/
/// https://transparencia.al.go.leg.br/
/// </summary>
public class Goias : ImportadorBase
{
    public Goias(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarGoias(serviceProvider);
        importadorDespesas = new ImportadorDespesasGoias(serviceProvider);
    }
}

public class ImportadorDespesasGoias : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

    public ImportadorDespesasGoias(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.go.leg.br/",
            Estado = Estado.Goias,
            ChaveImportacao = ChaveDespesaTemp.Nome
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}api/transparencia/verbas_indenizatorias?ano={ano}&mes={mes}&por_pagina=100";
        var restClient = new RestClient();

        var request = new RestRequest(address);
        request.AddHeader("Accept", "application/json");

        RestResponse resParlamentares = restClient.GetWithAutoRetry(request);
        List<DespesasGoias> lstDeputadosRS = JsonSerializer.Deserialize<List<DespesasGoias>>(resParlamentares.Content);

        foreach (DespesasGoias item in lstDeputadosRS)
        {
            var id = item.Deputado.Id;

            address = $"{config.BaseAddress}api/transparencia/verbas_indenizatorias/exibir?ano={ano}&deputado_id={id}&mes={mes}";
            request.Resource = address;

            RestResponse resDespesa = restClient.GetWithAutoRetry(request);
            DespesasGoias despesa = JsonSerializer.Deserialize<DespesasGoias>(resDespesa.Content);
            if (despesa.Grupos is null) continue;

            foreach (var grupo in despesa.Grupos)
                foreach (var sub in grupo.Subgrupos)
                    foreach (var lancamento in sub.Lancamentos)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = despesa.Deputado.Nome.Trim().ToTitleCase(),
                            Cpf = id.ToString(), // gabinete
                            Ano = (short)ano,
                            TipoDespesa = grupo.Descricao,
                            Valor = Convert.ToDecimal(lancamento.Fornecedor.ValorIndenizado, cultureInfo),
                            DataEmissao = lancamento.Fornecedor.Data,
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(lancamento.Fornecedor.CnpjCpf),
                            Empresa = lancamento.Fornecedor.Nome,
                            Documento = lancamento.Fornecedor.Numero
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
        }
    }
}

public class ImportadorParlamentarGoias : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarGoias(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://portal.al.go.leg.br/deputados/em-exercicio",
            SeletorListaParlamentares = "#tabela-deputados table tbody tr",
            Estado = Estado.Goias,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var colunas = parlamentar.QuerySelectorAll("td");
        var urlPerfil = (colunas[0].QuerySelector("a") as IHtmlAnchorElement).Href;
        var matricula = Convert.ToUInt32(urlPerfil.Split(@"/").Last());

        var deputado = GetDeputadoByMatriculaOrNew(matricula);

        deputado.IdPartido = BuscarIdPartido(colunas[1].TextContent.Split('(')[1].Split(')')[0].Trim());
        deputado.UrlPerfil = urlPerfil;
        deputado.NomeParlamentar = colunas[0].TextContent.Trim().ToTitleCase().ReduceWhitespace();
        deputado.Telefone = string.Join(",", colunas[2].QuerySelectorAll("a").Select(x => x.TextContent.Trim()));

        ImportacaoUtils.MapearRedeSocial(deputado, colunas[3].QuerySelectorAll("a"));

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector(".perfil__foto img") as IHtmlImageElement)?.Source;

        //var perfilPessoal = detalhes.QuerySelectorAll(".perfil-biografico__texto p")[0];
        //deputado.Naturalidade = null;
        //deputado.Nascimento = null;
        //deputado.Escolaridade = null;
        //deputado.Profissao = null;
    }
}

public class DeputadoGoias
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("partido")]
    public PartidoGO Partido { get; set; }

    [JsonPropertyName("foto")]
    public string Foto { get; set; }

    [JsonPropertyName("twitter")]
    public string Twitter { get; set; }

    [JsonPropertyName("facebook")]
    public string Facebook { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("sala")]
    public string Sala { get; set; }
}

public class Fornecedor
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("cnpj_cpf")]
    public string CnpjCpf { get; set; }

    [JsonPropertyName("data")]
    public DateTime Data { get; set; }

    [JsonPropertyName("numero")]
    public string Numero { get; set; }

    [JsonPropertyName("valor_apresentado")]
    public string ValorApresentado { get; set; }

    [JsonPropertyName("valor_indenizado")]
    public string ValorIndenizado { get; set; }
}

public class Grupo
{
    [JsonPropertyName("descricao")]
    public string Descricao { get; set; }

    //[JsonPropertyName("valor_apresentado")]
    //public object ValorApresentado { get; set; }

    //[JsonPropertyName("valor_indenizado")]
    //public object ValorIndenizado { get; set; }

    [JsonPropertyName("subgrupos")]
    public List<Subgrupo> Subgrupos { get; set; }
}

public class Lancamento
{
    [JsonPropertyName("fornecedor")]
    public Fornecedor Fornecedor { get; set; }
}

public class PartidoGO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("numero")]
    public int Numero { get; set; }

    [JsonPropertyName("sigla")]
    public string Sigla { get; set; }

    [JsonPropertyName("nome_presidente")]
    public string NomePresidente { get; set; }

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("ambito")]
    public string Ambito { get; set; }

    [JsonPropertyName("fone")]
    public string Fone { get; set; }

    [JsonPropertyName("fone2")]
    public string Fone2 { get; set; }

    [JsonPropertyName("fax")]
    public string Fax { get; set; }

    [JsonPropertyName("criado_por")]
    public int CriadoPor { get; set; }

    [JsonPropertyName("atualizado_por")]
    public int AtualizadoPor { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("site")]
    public string Site { get; set; }

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }
}

public class DespesasGoias
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ano")]
    public int Ano { get; set; }

    [JsonPropertyName("mes")]
    public int Mes { get; set; }

    [JsonPropertyName("deputado")]
    public DeputadoGoias Deputado { get; set; }

    //[JsonPropertyName("valor_apresentado")]
    //public decimal ValorApresentado { get; set; }

    //[JsonPropertyName("valor_indenizado")]
    //public decimal ValorIndenizado { get; set; }

    [JsonPropertyName("grupos")]
    public List<Grupo> Grupos { get; set; }
}

public class Subgrupo
{
    [JsonPropertyName("descricao")]
    public string Descricao { get; set; }

    //[JsonPropertyName("valor_apresentado")]
    //public decimal ValorApresentado { get; set; }

    //[JsonPropertyName("valor_indenizado")]
    //public decimal ValorIndenizado { get; set; }

    [JsonPropertyName("lancamentos")]
    public List<Lancamento> Lancamentos { get; set; }
}

