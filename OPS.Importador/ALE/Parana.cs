using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp;
using AngleSharp.Dom;
using Dapper;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using RestSharp;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado do Paraná
/// https://consultas.assembleia.pr.leg.br/#/ressarcimento
/// </summary>
public class Parana : ImportadorBase
{
    public Parana(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarParana(serviceProvider);
        importadorDespesas = new ImportadorDespesasParana(serviceProvider);
    }
}

public class ImportadorDespesasParana : ImportadorDespesasRestApiMensal
{
    public ImportadorDespesasParana(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Parana,
            ChaveImportacao = ChaveDespesaTemp.CpfParcial
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());

        var address = $"https://consultas.assembleia.pr.leg.br/api/public/ressarcimento/ressarcimentos/{mes}/{ano}";
        var restClientOptions = new RestClientOptions()
        {
            ThrowOnAnyError = true,
            MaxTimeout = (int)TimeSpan.FromMinutes(5).TotalMicroseconds
        };

        var restClient = new RestClient(restClientOptions);
        //restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        var request = new RestRequest(address);
        request.AddHeader("Accept", "application/json");

        RestResponse resParlamentares = restClient.GetWithAutoRetry(request);
        DespesaPR objDespesaPR = JsonSerializer.Deserialize<DespesaPR>(resParlamentares.Content, options);

        foreach (var itemDespesa in objDespesaPR.Despesas)
        {
            string nomePolitico = ImportarParlamantar(itemDespesa);

            foreach (var despesaMensal in itemDespesa.DespesasAnuais?[0]?.DespesasMensais?[0]?.Despesas)
                foreach (var despesa in despesaMensal.ItensDespesa)
                {
                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = nomePolitico,
                        Cpf = itemDespesa.Parlamentar.Codigo.ToString(),
                        Ano = (short)despesa.Exercicio,
                        TipoDespesa = despesa.TipoDespesa.Descricao,
                        Valor = (decimal)(despesa.Valor - despesa.ValorDevolucao),
                        DataEmissao = despesa.Data,
                        CnpjCpf = despesa.Fornecedor?.Documento,
                        Empresa = despesa.Fornecedor?.Nome,
                        Documento = $"{despesa.NumeroDocumento} [{despesa.Codigo}/{despesa.Numero}]",
                        Observacao = despesa.Descricao
                    };

                    if (despesa.Transporte != null)
                    {
                        var t = despesa.Transporte;
                        var v = t.Veiculo;

                        despesaTemp.Observacao =
                            $"{t.Descricao}; Veículo: {v.Placa}/{v.Modelo}; Distância: {t.Distancia:N0)}; Periodo: {t.DataSaida:dd/MM/yyyy} à {t.DataChegada:dd/MM/yyyy}";
                    }

                    if (despesa.Diaria != null)
                    {
                        var d = despesa.Diaria;

                        despesaTemp.Observacao =
                            $"{d.Descricao}; Diárias: {d.NumeroDiarias:N1}; Região: {d.Regiao}";
                    }

                    InserirDespesaTemp(despesaTemp);
                }
        }
    }

    private string ImportarParlamantar(Despesas itemDespesa)
    {
        var parlamentar = itemDespesa.Parlamentar;
        var nomePolitico = parlamentar.NomePolitico.Replace("DEPUTADO ", "").Replace("DEPUTADA ", "");

        var matricula = (uint)parlamentar.Codigo;
        var deputado = GetDeputadoByMatriculaOrNew(matricula);

        var IdPartido = connection.GetList<Partido>(new { sigla = parlamentar.Partido.Replace("REPUB", "REPUBLICANOS").Replace("CDN", "CIDADANIA") }).FirstOrDefault()?.Id;
        if (IdPartido == null)
            throw new Exception("Partido Inexistenete");

        //deputado.UrlPerfil = $"http://www.assembleia.pr.leg.br/deputados/perfil/{parlamentar.Codigo}";
        deputado.NomeParlamentar = nomePolitico;
        deputado.NomeCivil = parlamentar.Nome;
        deputado.IdPartido = IdPartido.Value;
        deputado.Sexo = parlamentar.NomePolitico.StartsWith("DEPUTADO") ? "M" : "F";

        if (deputado.Id == 0)
            connection.Insert(deputado);
        else
            connection.Update(deputado);

        return nomePolitico;
    }

    private class Despesas
    {
        [JsonPropertyName("parlamentar")]
        public Parlamentar Parlamentar { get; set; }

        [JsonPropertyName("despesasAnuais")]
        public List<DespesasAnuais> DespesasAnuais { get; set; }

        [JsonPropertyName("tipoDespesa")]
        public TipoDespesa TipoDespesa { get; set; }

        [JsonPropertyName("itensDespesa")]
        public List<ItensDespesa> ItensDespesa { get; set; }
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
        public int Exercicio { get; set; }

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

    private class DespesaPR
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; set; }

        [JsonPropertyName("erro")]
        public object Erro { get; set; }

        [JsonPropertyName("valor")]
        public object Valor { get; set; }

        [JsonPropertyName("despesas")]
        public List<Despesas> Despesas { get; set; }
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

//public class ImportadorParlamentarParana : ImportadorParlamentarCrawler
//{

//    public ImportadorParlamentarParana(IServiceProvider serviceProvider) : base(serviceProvider)
//    {
//        Configure(new ImportadorParlamentarCrawlerConfig()
//        {
//            BaseAddress = "",
//            SeletorListaParlamentares = "",
//            Estado = Estado.Parana,
//        });
//    }

//    public override DeputadoEstadual ColetarDadosLista(IElement document)
//    {
//        throw new NotImplementedException();
//    }

//    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
//    {
//        throw new NotImplementedException();
//    }
//}


