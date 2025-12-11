using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Comum;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;
using RestSharp;

namespace OPS.Importador.Assembleias;

/// <summary>
/// Assembleia Legislativa do Estado do Paraná
/// https://consultas.assembleia.pr.leg.br/#/ressarcimento
/// </summary>
public class Parana : ImportadorBase
{
    public Parana(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarParana(serviceProvider);
        importadorDespesas = new ImportadorDespesasParana(serviceProvider);
    }
}

public class ImportadorDespesasParana : ImportadorDespesasRestApiAnual
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasParana(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://consultas.assembleia.pr.leg.br/api/",
            Estado = Estado.Parana,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        // https://consultas.assembleia.pr.leg.br/#/ressarcimento
        ImportarCotaParlamentar(ano);

        //OpenPageWaitForCaptchaAndClickConsultar(ano, mes);

        // TODO: Para importar as diarias precisamos primeiro importar a posição do parlamentar (ex: 4ª SECRETARIA) e o pessoal do gabinete;
        // http://transparencia.assembleia.pr.leg.br/pessoal/comissionados
        // http://transparencia.assembleia.pr.leg.br/pessoal/estaveis
        //ImportarDiarias(ano, mes);
    }

    private void ImportarDiarias(int ano, int mes)
    {
        var indice = 0;
        var idxPgto = indice++;
        var idxDataSaida = indice++;
        var idxDataRetorno = indice++;
        var idxProtocolo = indice++;
        var idxOrcamento = indice++;
        var idxEmitidoPara = indice++;
        var idxValor = indice++;
        var idxQuantidade = indice++;
        var idxMotivo = indice++;
        var idxDestino = indice++;

        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());

        var address = $"http://transparencia.assembleia.pr.leg.br/api/diarias?ano={ano}&mes={mes}";
        var restClient = CreateHttpClient();

        var request = new RestRequest(address);
        request.AddHeader("Accept", "application/json");

        RestResponse resDiarias = restClient.Get(request);
        List<List<string>> diarias = JsonSerializer.Deserialize<List<List<string>>>(resDiarias.Content, options);

        foreach (var diaria in diarias)
        {
            if (diaria[idxEmitidoPara].StartsWith("GABINETE MILITAR")) continue;
            // TODO: Emitido para pode ser um comissionado e ainda não temos essa info.

            var despesaTemp = new CamaraEstadualDespesaTemp()
            {
                Nome = diaria[idxEmitidoPara].Split("-")[0].Trim().ToTitleCase(),
                Ano = (short)ano,
                Mes = (short)mes,
                TipoDespesa = "Diárias",
                Valor = Convert.ToDecimal(diaria[idxValor], cultureInfo),
                DataEmissao = Convert.ToDateTime(diaria[idxDataSaida], cultureInfo),
                Documento = diaria[idxPgto],
                Observacao = $"Trecho: {diaria[idxDestino]}; Protocolo: {diaria[idxProtocolo]} Orçamento: {diaria[idxOrcamento]}"
            };


            InserirDespesaTemp(despesaTemp);
        }
    }

    private void ImportarCotaParlamentar(int ano)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());


        var files = Directory.EnumerateFiles(tempPath, $"{ano}*");
        foreach (var item in files)
        {
            var content = File.ReadAllText(item);
            if (content.Contains("invalid-recaptcha"))
            {
                logger.LogError("Arquivo invalido: {CaminhoArquivo}", item);
                File.Delete(item);
                continue;
            }

            DespesasPR objDespesaPR = JsonSerializer.Deserialize<DespesasPR>(content, options);

            foreach (var parlamentarDespesa in objDespesaPR.Despesas)
            {
                foreach (var despesa in parlamentarDespesa.DespesasAnuais?[0]?.DespesasMensais?[0]?.Despesas)
                {
                    foreach (var itensDespesa in despesa.ItensDespesa)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = parlamentarDespesa.Parlamentar.NomePolitico.Replace("DEPUTADA", "").Replace("DEPUTADO", "").Trim().ToTitleCase(),
                            Cpf = parlamentarDespesa.Parlamentar.Codigo.ToString(),
                            Ano = (short)itensDespesa.Exercicio,
                            TipoDespesa = itensDespesa.TipoDespesa.Descricao,
                            Valor = (decimal)(itensDespesa.Valor - itensDespesa.ValorDevolucao),
                            DataEmissao = itensDespesa.Data,
                            CnpjCpf = itensDespesa.Fornecedor?.Documento,
                            Empresa = itensDespesa.Fornecedor?.Nome,
                            Documento = $"{itensDespesa.NumeroDocumento} [{itensDespesa.Codigo}/{itensDespesa.Numero}]",
                            Observacao = itensDespesa.Descricao
                        };

                        if (itensDespesa.Transporte != null)
                        {
                            var t = itensDespesa.Transporte;
                            var v = t.Veiculo;

                            despesaTemp.Observacao =
                                $"{t.Descricao}; Veículo: {v.Placa}/{v.Modelo}; Distância: {t.Distancia:N0)}; Periodo: {t.DataSaida:dd/MM/yyyy} à {t.DataChegada:dd/MM/yyyy}";
                        }

                        if (itensDespesa.Diaria != null)
                        {
                            var d = itensDespesa.Diaria;

                            despesaTemp.Observacao =
                                $"{d.Descricao}; Diárias: {d.NumeroDiarias:N1}; Região: {d.Regiao}";
                        }

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }
        }
    }

    //private void ImportarCotaParlamentar(int ano, int mes)
    //{
    //    // Use Selenium to open the page and wait for cacpcha solve and click in "Consultar" on https://consultas.assembleia.pr.leg.br/#/ressarcimento
    //    //

    //    var address = $"{config.BaseAddress}public/ressarcimento/ressarcimentos/{mes}/{ano}";

    //    ParlamentaresPR objParlamentaresPR = RestApiGetWithCustomDateConverter<ParlamentaresPR>(address);

    //    foreach (var itemParlamentar in objParlamentaresPR.Parlamentares)
    //    {
    //        string nomePolitico = ImportarParlamentar(itemParlamentar);

    //        var newAddress = address.Replace("ressarcimentos", "despesas-ressarcimento") + "/" + itemParlamentar.Parlamentar.Codigo;
    //        DespesasPR objDespesaPR = RestApiGetWithCustomDateConverter<DespesasPR>(newAddress);
    //        if (!objDespesaPR.Sucesso) continue;

    //        foreach (var parlamentarDespesa in objDespesaPR.Despesas)
    //        {
    //            foreach (var despesa in parlamentarDespesa.DespesasAnuais?[0]?.DespesasMensais?[0]?.Despesas)
    //            {
    //                foreach (var itensDespesa in despesa.ItensDespesa)
    //                {
    //                    var despesaTemp = new CamaraEstadualDespesaTemp()
    //                    {
    //                        Nome = nomePolitico.ToTitleCase(),
    //                        Cpf = parlamentarDespesa.Parlamentar.Codigo.ToString(),
    //                        Ano = (short)itensDespesa.Exercicio,
    //                        TipoDespesa = itensDespesa.TipoDespesa.Descricao,
    //                        Valor = (decimal)(itensDespesa.Valor - itensDespesa.ValorDevolucao),
    //                        DataEmissao = itensDespesa.Data,
    //                        CnpjCpf = itensDespesa.Fornecedor?.Documento,
    //                        Empresa = itensDespesa.Fornecedor?.Nome,
    //                        Documento = $"{itensDespesa.NumeroDocumento} [{itensDespesa.Codigo}/{itensDespesa.Numero}]",
    //                        Observacao = itensDespesa.Descricao
    //                    };

    //                    if (itensDespesa.Transporte != null)
    //                    {
    //                        var t = itensDespesa.Transporte;
    //                        var v = t.Veiculo;

    //                        despesaTemp.Observacao =
    //                            $"{t.Descricao}; Veículo: {v.Placa}/{v.Modelo}; Distância: {t.Distancia:N0)}; Periodo: {t.DataSaida:dd/MM/yyyy} à {t.DataChegada:dd/MM/yyyy}";
    //                    }

    //                    if (itensDespesa.Diaria != null)
    //                    {
    //                        var d = itensDespesa.Diaria;

    //                        despesaTemp.Observacao =
    //                            $"{d.Descricao}; Diárias: {d.NumeroDiarias:N1}; Região: {d.Regiao}";
    //                    }

    //                    InserirDespesaTemp(despesaTemp);
    //                }
    //            }
    //        }
    //    }
    //}

    private string ImportarParlamentar(Parlamentares itemDespesa)
    {
        var parlamentar = itemDespesa.Parlamentar;
        var nomeParlamentar = parlamentar.NomePolitico.Replace("DEPUTADO ", "").Replace("DEPUTADA ", "").Trim().ToTitleCase();

        var matricula = (uint)parlamentar.Codigo;
        var deputado = GetDeputadoByMatriculaOrNew(matricula);

        var IdPartido = connection.GetList<Partido>(new { sigla = parlamentar.Partido.Replace("REPUB", "REPUBLICANOS").Replace("CDN", "CIDADANIA") }).FirstOrDefault()?.Id;
        if (IdPartido == null && deputado.IdPartido == null)
            throw new Exception($"Partido '{parlamentar.Partido}' Inexistenete");

        //deputado.UrlPerfil = $"http://www.assembleia.pr.leg.br/deputados/perfil/{parlamentar.Codigo}";
        deputado.NomeParlamentar = nomeParlamentar;
        deputado.NomeCivil = parlamentar.Nome.ToTitleCase();

        deputado.IdPartido ??= IdPartido;
        deputado.Sexo = parlamentar.NomePolitico.StartsWith("DEPUTADO") ? "M" : "F";

        if (deputado.Id == 0)
            connection.Insert(deputado);
        else
            connection.Update(deputado);

        return nomeParlamentar;
    }

    private class Parlamentares
    {
        [JsonPropertyName("parlamentar")]
        public Parlamentar Parlamentar { get; set; }

        //[JsonPropertyName("despesasAnuais")]
        //public List<DespesasAnuais> DespesasAnuais { get; set; }

        //[JsonPropertyName("tipoDespesa")]
        //public TipoDespesa TipoDespesa { get; set; }

        //[JsonPropertyName("itensDespesa")]
        //public List<ItensDespesa> ItensDespesa { get; set; }
    }

    private class ParlamentarDespesas
    {
        [JsonPropertyName("parlamentar")]
        public Parlamentar Parlamentar { get; set; }

        [JsonPropertyName("despesasAnuais")]
        public List<DespesasAnuais> DespesasAnuais { get; set; }

        //[JsonPropertyName("tipoDespesa")]
        //public TipoDespesa TipoDespesa { get; set; }

        //[JsonPropertyName("itensDespesa")]
        //public List<ItensDespesa> ItensDespesa { get; set; }
    }

    private class DespesasAnuais
    {
        [JsonPropertyName("exercicio")]
        public int Exercicio { get; set; }

        [JsonPropertyName("despesasMensais")]
        public List<DespesasMensais> DespesasMensais { get; set; }

        [JsonPropertyName("verba")]
        public object Verba { get; set; }
    }

    private class DespesasMensais
    {
        [JsonPropertyName("mes")]
        public int Mes { get; set; }

        [JsonPropertyName("despesas")]
        public List<Despesas> Despesas { get; set; }

        [JsonPropertyName("verba")]
        public double Verba { get; set; }

        [JsonPropertyName("saldo")]
        public double Saldo { get; set; }

        [JsonPropertyName("total")]
        public double Total { get; set; }
    }

    private class Despesas
    {
        [JsonPropertyName("itensDespesa")]
        public List<ItensDespesa> ItensDespesa { get; set; }
    }

    private class Diaria
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        [JsonPropertyName("numeroDiarias")]
        public double NumeroDiarias { get; set; }

        [JsonPropertyName("regiao")]
        public string Regiao { get; set; }
    }

    private class FornecedorPR
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("documento")]
        public string Documento { get; set; }

        [JsonPropertyName("matricula")]
        public string Matricula { get; set; }
    }

    private class ItensDespesa
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("exercicio")]
        public int? Exercicio { get; set; }

        [JsonPropertyName("mes")]
        public int Mes { get; set; }

        [JsonPropertyName("tipoDespesa")]
        public TipoDespesa TipoDespesa { get; set; }

        [JsonPropertyName("valor")]
        public double Valor { get; set; }

        [JsonPropertyName("situacao")]
        public string Situacao { get; set; }

        [JsonPropertyName("data")]
        public DateTime Data { get; set; }

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        [JsonPropertyName("fornecedor")]
        public FornecedorPR Fornecedor { get; set; }

        [JsonPropertyName("transporte")]
        public Transporte Transporte { get; set; }

        [JsonPropertyName("diaria")]
        public Diaria Diaria { get; set; }

        [JsonPropertyName("numero")]
        public int Numero { get; set; }

        [JsonPropertyName("valorDevolucao")]
        public double ValorDevolucao { get; set; }

        [JsonPropertyName("anexos")]
        public object Anexos { get; set; }

        [JsonPropertyName("numeroDocumento")]
        public string NumeroDocumento { get; set; }
    }

    private class Parlamentar
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("nomePolitico")]
        public string NomePolitico { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("numeroGabinete")]
        public object NumeroGabinete { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }
    }

    private class ParlamentaresPR
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; set; }

        [JsonPropertyName("erro")]
        public object Erro { get; set; }

        [JsonPropertyName("valor")]
        public object Valor { get; set; }

        [JsonPropertyName("despesas")]
        public List<Parlamentares> Parlamentares { get; set; }
    }

    private class DespesasPR
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; set; }

        [JsonPropertyName("erro")]
        public object Erro { get; set; }

        [JsonPropertyName("valor")]
        public object Valor { get; set; }

        [JsonPropertyName("despesas")]
        public List<ParlamentarDespesas> Despesas { get; set; }
    }

    private class TipoDespesa
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        [JsonPropertyName("categoria")]
        public string Categoria { get; set; }
    }

    private class Transporte
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        [JsonPropertyName("veiculo")]
        public Veiculo Veiculo { get; set; }

        [JsonPropertyName("distancia")]
        public double Distancia { get; set; }

        [JsonPropertyName("dataSaida")]
        public DateTime DataSaida { get; set; }

        [JsonPropertyName("dataChegada")]
        public DateTime DataChegada { get; set; }
    }

    private class Veiculo
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("numero")]
        public int Numero { get; set; }

        [JsonPropertyName("placa")]
        public string Placa { get; set; }

        [JsonPropertyName("modelo")]
        public string Modelo { get; set; }
    }
}

public class ImportadorParlamentarParana : ImportadorParlamentarCrawler
{
    private readonly List<DeputadoEstadual> deputados;

    public ImportadorParlamentarParana(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            // https://www.assembleia.pr.leg.br/deputados/legislatura/19a-legislatura-3a-e-4a-sessoes-legislativas
            BaseAddress = "https://www.assembleia.pr.leg.br/deputados/conheca",
            SeletorListaParlamentares = ".pg-deps .dep",
            Estado = Estado.Parana,
        });

        deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var nomeParlamentar = parlamentar.QuerySelector(".nome-deps span").TextContent.Trim().ReduceWhitespace();
        var nomeParlamentarLimpo = Utils.RemoveAccents(nomeParlamentar);
        var urlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;

        var deputado = deputados.Find(x =>
            Utils.RemoveAccents(x.NomeImportacao ?? "").Equals(nomeParlamentarLimpo, StringComparison.InvariantCultureIgnoreCase) ||
            Utils.RemoveAccents(x.NomeParlamentar).Equals(nomeParlamentarLimpo, StringComparison.InvariantCultureIgnoreCase) ||
            Utils.RemoveAccents(x.NomeCivil ?? "").Equals(nomeParlamentarLimpo, StringComparison.InvariantCultureIgnoreCase) ||
            x.UrlPerfil == urlPerfil
        );

        if (deputado == null)
        {
            deputado = new DeputadoEstadual()
            {
                NomeParlamentar = nomeParlamentar,
                IdEstado = (ushort)config.Estado.GetHashCode()
            };
        }

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = deputado.NomeParlamentar;

        deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".nome-deps .partido").TextContent.Trim());
        deputado.UrlPerfil = urlPerfil;
        deputado.UrlFoto = (parlamentar.QuerySelector(".foto-deps img") as IHtmlImageElement)?.Source;

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.Telefone = subDocument.QuerySelector(".tel span")?.TextContent;

        var redesSociais = subDocument.QuerySelectorAll(".compartilhamento-redes a,.link-site");
        if (redesSociais.Length > 0)
            ImportacaoUtils.MapearRedeSocial(deputado, redesSociais);
    }
}


