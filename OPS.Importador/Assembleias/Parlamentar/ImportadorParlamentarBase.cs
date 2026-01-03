using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias;
using RestSharp;
using Serilog;

namespace OPS.Importador.Assembleias.Parlamentar
{
    public abstract class ImportadorParlamentarBase : IImportadorParlamentar
    {
        protected readonly ILogger<ImportadorParlamentarBase> logger;
        protected readonly IDbConnection connection;
        protected ImportadorParlamentarConfig config;

        public int registrosInseridos { get; private set; } = 0;
        public int registrosAtualizados { get; private set; } = 0;

        private HttpClient _httpClient;
        public HttpClient httpClient { get { return _httpClient ??= httpClientFactory.CreateClient("ResilientClient"); } }

        private IHttpClientFactory httpClientFactory { get; }

        public ImportadorParlamentarBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorParlamentarBase>>();
            connection = serviceProvider.GetService<IDbConnection>();

            httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }

        public void Configure(ImportadorParlamentarConfig config)
        {
            this.config = config;
        }

        public abstract Task Importar();

        public virtual Task DownloadFotos()
        {
            return Task.CompletedTask;
        }

        public ushort BuscarIdPartido(string partido)
        {
            if (partido == "PATRI" || partido.Equals("PATRIOTAS", StringComparison.InvariantCultureIgnoreCase)) partido = "PATRIOTA";
            else if (partido == "PTC") partido = "AGIR"; // https://agir36.com.br/sobre-o-partido/
            else if (partido == "REPUB" || partido == "REP" || partido == "REPUBLICAN" || partido == "PRB") partido = "REPUBLICANOS";
            else if (partido == "PR") partido = "PL"; // Partido da República
            else if (partido == "Podemos" || partido == "POD") partido = "PODE";
            else if (partido == "UNIÃO BRASIL (UNIÃO)" || partido == "UB") partido = "UNIÃO";
            else if (partido == "CIDA" || partido == "CDN" || partido == "PPS") partido = "CIDADANIA";
            else if (partido == "PSDC") partido = "DC"; // Democracia Cristã
            else if (partido == "PTR") partido = "PP"; // Progressistas
            else if (partido.Equals("PC DO B", StringComparison.InvariantCultureIgnoreCase)) partido = "PCdoB";
            else if (partido.Contains("PROGRESSISTA") || partido == "Partido Progressista") partido = "PP"; // Progressistas
            else if (partido.Contains("SOLIDARIEDADE") || partido == "SDD") partido = "SD"; // Solidariedade
            else if (partido.Contains("PARTIDO VERDE")) partido = "PV";
            else if (partido.Contains("PMN")) partido = "MOBILIZA";
            else if (partido.Contains("Não possui filiação") || string.IsNullOrEmpty(partido)) partido = "S.PART.";

            var IdPartido = connection.GetList<Partido>(new { Sigla = partido }).FirstOrDefault()?.Id;
            if (IdPartido == null)
            {
                IdPartido = connection.GetList<Partido>(new { Nome = partido }).FirstOrDefault()?.Id;
                if (IdPartido == null)
                {
                    Log.Error("Partido '{Partido}' Inexistenete", partido);
                    //throw new Exception($"Partido '{partido}' Inexistenete");
                    return 0; //  S.PART.
                }
            }

            return IdPartido.Value;
        }

        protected DeputadoEstadual GetDeputadoByNameOrNew(string nomeParlamentar)
        {
            var id = connection
                .GetList<DeputadoEstadualDepara>(new
                {
                    id_estado = config.Estado,
                    nome = nomeParlamentar
                })
                .Select(d => d.Id)
                .FirstOrDefault();

            if (id == 0)
            {
                return new DeputadoEstadual()
                {
                    IdEstado = (ushort)config.Estado,
                    NomeParlamentar = nomeParlamentar
                };
            }

            //var deputado = connection
            //    .GetList<DeputadoEstadual>(new
            //    {
            //        id_estado = config.Estado,
            //        nome_parlamentar = nomeParlamentar
            //    })
            //    .FirstOrDefault();

            //if (deputado == null)
            //    deputado = connection
            //    .GetList<DeputadoEstadual>(new
            //    {
            //        id_estado = config.Estado,
            //        nome_importacao = nomeParlamentar
            //    })
            //    .FirstOrDefault();

            return connection.Get<DeputadoEstadual>(id);
        }

        //protected DeputadoEstadual GetDeputadoByFullNameOrNew(string nome)
        //{
        //    var deputado = connection
        //        .GetList<DeputadoEstadual>(new
        //        {
        //            id_estado = config.Estado,
        //            nome_civil = nome
        //        })
        //        .FirstOrDefault();

        //    if (deputado != null)
        //        return deputado;

        //    return new DeputadoEstadual()
        //    {
        //        IdEstado = (ushort)config.Estado,
        //        NomeCivil = nome
        //    };
        //}

        protected DeputadoEstadual GetDeputadoByMatriculaOrNew(uint matricula)
        {
            var deputado = connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    matricula
                })
                .FirstOrDefault();

            if (deputado != null)
                return deputado;

            return new DeputadoEstadual()
            {
                IdEstado = (ushort)config.Estado,
                Matricula = matricula
            };
        }

        public void InsertOrUpdate(DeputadoEstadual deputado)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Deputado"] = JsonSerializer.Serialize(deputado) }))
            {
                if (deputado.Id == 0)
                {
                    deputado.Id = (uint)connection.Insert(deputado);
                    registrosInseridos++;

                    connection.Insert<uint?, DeputadoEstadualDepara>(new DeputadoEstadualDepara()
                    {
                        Id = deputado.Id,
                        Nome = deputado.NomeParlamentar,
                        IdEstado = deputado.IdEstado,
                    });
                }
                else
                {
                    connection.Update(deputado);
                    registrosAtualizados++;
                }
            }
        }

        public RestClient CreateHttpClient()
        {
            var options = new RestClientOptions()
            {
                ThrowOnAnyError = true
            };
            return new RestClient(httpClient, options);
        }

        public T RestApiGet<T>(string address)
        {
            var restClient = CreateHttpClient();

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            return restClient.Get<T>(request);
        }

        public T RestApiGetWithSqlTimestampConverter<T>(string address)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SqlTimestampConverter());

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            using RestClient client = CreateHttpClient();
            var response = client.Get(request);
            return JsonSerializer.Deserialize<T>(response.Content, options);

        }

        public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null)
        {
            var importacao = connection.GetList<Importacao>(new { nome = config.Estado.ToString() }).FirstOrDefault();
            if (importacao == null)
            {
                importacao = new Importacao()
                {
                    Chave = config.Estado.ToString()
                };
                importacao.Id = (ushort)connection.Insert(importacao);
            }

            if (pInicio != null)
            {
                importacao.ParlamentarInicio = pInicio.Value;
                importacao.ParlamentarFim = null;
            }
            if (pFim != null) importacao.ParlamentarFim = pFim.Value;

            connection.Update(importacao);
        }
    }

    public class ImportadorParlamentarConfig
    {
        public string BaseAddress { get; set; }

        public Estado Estado { get; set; }

    }
}
