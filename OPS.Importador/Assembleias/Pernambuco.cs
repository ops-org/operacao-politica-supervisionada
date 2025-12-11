using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Importador.Utilities;
using RestSharp;
using Dapper;
using System.Text.Json.Serialization;
using OPS.Core.Utilities;
using OPS.Core.Enumerator;
using OPS.Importador.Assembleias.Comum;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Assembleias.Despesa;

namespace OPS.Importador.Assembleias;

/// <summary>
/// Assembleia Legislativa do Estado do Pernambuco
/// https://www.alepe.pe.gov.br/
/// </summary>
public class Pernambuco : ImportadorBase
{
    public Pernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarPernambuco(serviceProvider);
        importadorDespesas = new ImportadorDespesasPernambuco(serviceProvider);
    }
}

public class ImportadorDespesasPernambuco : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasPernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.alepe.pe.gov.br/servicos/transparencia/",
            Estado = Estado.Pernambuco,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        List<RubricasPE> rubricas;
        //try
        //{
        //    rubricas = RestApiGetWithCustomDateConverter<List<RubricasPE>>($"{config.BaseAddress}adm/verbaindenizatoria-rubricas.php?ano={ano}");
        //}
        //catch (Exception)
        //{
            rubricas = new List<RubricasPE>
            {
                new RubricasPE(){ NumeroCategoria = "1", NomeCategoria = "IMÓVEIS"},
                new RubricasPE(){ NumeroCategoria = "2", NomeCategoria = "Locação de veículos"},
                new RubricasPE(){ NumeroCategoria = "3", NomeCategoria = "Assessoria Jurídica"},
                new RubricasPE(){ NumeroCategoria = "4", NomeCategoria = "CONSULTORIAS E TRABALHOS TÉCNICOS"},
                new RubricasPE(){ NumeroCategoria = "5", NomeCategoria = "Divulgação da atividade parlamentar"},
                new RubricasPE(){ NumeroCategoria = "6", NomeCategoria = "SERVIÇOS DE TELECOMUNICAÇÕES EM GERAL"},
                new RubricasPE(){ NumeroCategoria = "7", NomeCategoria = "SERVIÇOS E PRODUTOS POSTAIS"},
                new RubricasPE(){ NumeroCategoria = "9", NomeCategoria = "FORNECIMENTO DE ALIMENTAÇÃO DO PARLAMENTAR"},
                new RubricasPE(){ NumeroCategoria = "10", NomeCategoria = "SERVIÇOS DE SEGURANÇA"},
            };
        //}


        var deputados = RestApiGetWithCustomDateConverter<List<DeputadoPE>>($"{config.BaseAddress}dep/deputados.php?leg=-16"); // 2023-2026
        if (!deputados.Any())
        {
            logger.LogWarning("Nenhum parlamentar foi encontrado.");
            return;
        }

        foreach (var deputado in deputados)
        {
            var meses = RestApiGetWithCustomDateConverter<List<DespesaMesesPE>>($"{config.BaseAddress}adm/verbaindenizatoria-dep-meses.php?dep={deputado.Id}&ano={ano}");
            foreach (var mesComDespesa in meses)
            {
                var documentos = RestApiGetWithCustomDateConverter<List<DespesaDocumentosPE>>($"{config.BaseAddress}adm/verbaindenizatoria.php?dep={deputado.Id}&ano={ano}&mes={mesComDespesa.Mes}");
                if (!documentos.Any())
                {
                    logger.LogWarning("Não da documentos para o parlamentar {Parlamentar} em {Mes}/{Ano}", deputado.Nome, mesComDespesa.Mes, ano);
                    continue;
                }

                foreach (var documento in documentos)
                {
                    var despesas = RestApiGetWithCustomDateConverter<List<DespesaPE>>($"{config.BaseAddress}adm/verbaindenizatorianotas.php?docid={documento.Docid}");
                    if (!despesas.Any())
                    {
                        logger.LogWarning("Não da despesas para o parlamentar {Parlamentar} em {Mes}/{Ano}", deputado.Nome, mesComDespesa.Mes, ano);
                        continue;
                    }

                    foreach (var despesa in despesas)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = deputado.Nome,
                            Cpf = deputado.Id,
                            Ano = (short)ano,
                            Mes = Convert.ToInt16(mesComDespesa.Mes),
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(despesa.Cnpj),
                            Empresa = despesa.Empresa,
                            Documento = documento.Docid,
                            Valor = Convert.ToDecimal(despesa.Valor, cultureInfo),
                            DataEmissao = Convert.ToDateTime(despesa.Data, cultureInfo),
                            TipoDespesa = rubricas.First(x => x.NumeroCategoria == despesa.Rubrica).NomeCategoria,
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }
        }
    }

    public override void AjustarDados()
    {
        connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '11364583000156' WHERE cnpj_cpf = '11136458300015';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '02421421000111' WHERE cnpj_cpf = '02421421001255';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '18376563000306' WHERE cnpj_cpf = '18376663000306';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '18376563000306' WHERE cnpj_cpf = '18376563000366';

");
    }

    public class DeputadoPE
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("to_ascii")]
        public string ToAscii { get; set; }
    }


    public class DespesaMesesPE
    {
        [JsonPropertyName("mes")]
        public string Mes { get; set; }
    }

    public class DespesaDocumentosPE
    {
        [JsonPropertyName("docid")]
        public string Docid { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("ano")]
        public string Ano { get; set; }

        [JsonPropertyName("deputado")]
        public string Deputado { get; set; }

        [JsonPropertyName("mes")]
        public string Mes { get; set; }

        [JsonPropertyName("total")]
        public string Total { get; set; }
    }

    public class DespesaPE
    {
        [JsonPropertyName("rubrica")]
        public string Rubrica { get; set; }

        [JsonPropertyName("sequencial")]
        public string Sequencial { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; }

        [JsonPropertyName("empresa")]
        public string Empresa { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }
    }

    public class RubricasPE
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nome_categoria")]
        public string NomeCategoria { get; set; }

        [JsonPropertyName("data_criacao")]
        public string DataCriacao { get; set; }

        [JsonPropertyName("id_usuario")]
        public string IdUsuario { get; set; }

        [JsonPropertyName("ativo")]
        public string Ativo { get; set; }

        [JsonPropertyName("numero_categoria")]
        public string NumeroCategoria { get; set; }

        [JsonPropertyName("numero_romano")]
        public string NumeroRomano { get; set; }

        [JsonPropertyName("valor_categoria")]
        public string ValorCategoria { get; set; }
    }
}

public class ImportadorParlamentarPernambuco : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarPernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.alepe.pe.gov.br/parlamentares/",
            SeletorListaParlamentares = "#parlamentares-modo-fotos ul li",
            Estado = Estado.Pernambuco,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".parlamentares-nome").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (item.QuerySelector("a") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (item.QuerySelector("img") as IHtmlImageElement)?.Source;

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var cabecalho = subDocument.QuerySelector(".parlamentares-view-header");
        var nomeparlamentar = cabecalho.QuerySelector(".text .title").TextContent.Trim();

        deputado.IdPartido = BuscarIdPartido(cabecalho.QuerySelector(".text .subtitle").TextContent.Trim());

        var info1 = subDocument.QuerySelectorAll(".parlamentares-view-info dl.first dd");
        deputado.Naturalidade = info1[1].TextContent.Trim();
        deputado.Email = info1[2].TextContent.Trim();
        deputado.Site = info1[3].TextContent.Trim();

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = info1[0].TextContent.Trim().ToTitleCase();

        if (info1.Length > 4)
            ImportacaoUtils.MapearRedeSocial(deputado, info1[4].QuerySelectorAll("a"));

        var info2 = subDocument.QuerySelectorAll(".parlamentares-view-info dl.last dd");
        //var aniversario = info2[0].TextContent.Trim(); // Falta ano
        deputado.Profissao = info2[1].TextContent.Trim().ToTitleCase();
        deputado.Telefone = info2[2].TextContent.Trim();
        //var gabinete = info2[3].TextContent.Trim();
    }
}
