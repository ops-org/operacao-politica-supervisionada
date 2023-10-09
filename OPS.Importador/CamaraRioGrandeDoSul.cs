using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using RestSharp;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa do Estado de Rio Grande do Sul
    /// </summary>
    public class CamaraRioGrandeDoSul : ImportadorCotaParlamentarBase
    {
        public CamaraRioGrandeDoSul(ILogger<CamaraRioGrandeDoSul> logger, IConfiguration configuration, IDbConnection connection) :
            base("RS", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var address = $"https://ww4.al.rs.gov.br:5000/listarDestaqueDeputados";
            var restClient = new RestClient();
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var request = new RestRequest(address, Method.GET);
            request.AddHeader("Accept", "application/json");

            IRestResponse resParlamentares = restClient.ExecuteWithAutoRetry(request);
            DeputadosRS objDeputadosRS = JsonSerializer.Deserialize<DeputadosRS>(resParlamentares.Content);

            foreach (var parlamentar in objDeputadosRS.Lista)
            {
                var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = parlamentar.SiglaPartido }).FirstOrDefault()?.Id;
                if (IdPartido == null)
                    throw new Exception("Partido Inexistenete");

                var deputado = new DeputadoEstadual();
                deputado.Matricula = (UInt32)parlamentar.IdDeputado;
                deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
                deputado.NomeParlamentar = parlamentar.NomeDeputado;
                deputado.IdEstado = (ushort)idEstado;
                deputado.IdPartido = IdPartido.Value;
                deputado.Email = parlamentar.EmailDeputado;
                deputado.Telefone = parlamentar.TelefoneDeputado;
                deputado.UrlFoto = parlamentar.FotoGrandeDeputado;

                var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, matricula = parlamentar.IdDeputado }).FirstOrDefault()?.Id;
                if (IdDeputado == null)
                    connection.Insert(deputado);
                else
                {
                    deputado.Id = IdDeputado.Value;
                    connection.Update(deputado);
                }
            }
        }

        protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        {

        }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class DeputadoRS
    {
        [JsonPropertyName("codigoPro")]
        public int CodigoPro { get; set; }

        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("nomeDeputado")]
        public string NomeDeputado { get; set; }

        [JsonPropertyName("emailDeputado")]
        public string EmailDeputado { get; set; }

        [JsonPropertyName("telefoneDeputado")]
        public string TelefoneDeputado { get; set; }

        [JsonPropertyName("siglaPartido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("nomePartido")]
        public string NomePartido { get; set; }

        [JsonPropertyName("codStatus")]
        public int CodStatus { get; set; }

        [JsonPropertyName("fotoGrandeDeputado")]
        public string FotoGrandeDeputado { get; set; }
    }

    public class DeputadosRS
    {
        [JsonPropertyName("lista")]
        public List<DeputadoRS> Lista { get; set; }
    }


}
