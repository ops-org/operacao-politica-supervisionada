using System.Data;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core.Utilities;
using OPS.Infraestrutura;
using RestSharp;
using Serilog;

namespace OPS.Importador.Assembleias.Parlamentar
{
    public abstract class ImportadorParlamentarBase : IImportadorParlamentar
    {
        protected readonly ILogger<ImportadorParlamentarBase> logger;
        protected readonly IDbConnection connection;
        protected readonly AppDbContext dbContext;
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
            dbContext = serviceProvider.GetService<AppDbContext>();

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

        public byte BuscarIdPartido(string partido)
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

            var IdPartido = dbContext.Partidos.FirstOrDefault(x => x.Sigla == partido)?.Id;
            if (IdPartido == null)
            {
                IdPartido = dbContext.Partidos.FirstOrDefault(x => x.Nome == partido)?.Id;
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
            var id = dbContext.DeputadoEstadualDeparas
                .Where(d => d.IdEstado == config.Estado.GetHashCode() && d.Nome == nomeParlamentar)
                .Select(d => d.Id)
                .FirstOrDefault();

            if (id == 0)
            {
                return new DeputadoEstadual()
                {
                    IdEstado = (byte)config.Estado,
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

            return dbContext.DeputadosEstaduais.Find(id);
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
        //        IdEstado = (short)config.Estado,
        //        NomeCivil = nome
        //    };
        //}

        protected DeputadoEstadual GetDeputadoByMatriculaOrNew(int matricula)
        {
            var deputado = dbContext.DeputadosEstaduais
                .Where(d => d.IdEstado == config.Estado.GetHashCode() && d.Matricula == matricula)
                .FirstOrDefault();

            if (deputado != null)
                return deputado;

            return new DeputadoEstadual()
            {
                IdEstado = (byte)config.Estado,
                Matricula = matricula
            };
        }

        public void InsertOrUpdate(DeputadoEstadual deputado)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Deputado"] = JsonSerializer.Serialize(deputado) }))
            {
                if (deputado.Id == 0)
                {
                    dbContext.DeputadosEstaduais.Add(deputado);
                    dbContext.SaveChanges();
                    registrosInseridos++;

                    dbContext.DeputadoEstadualDeparas.Add(new DeputadoEstadualDepara()
                    {
                        Id = deputado.Id,
                        Nome = deputado.NomeParlamentar,
                        IdEstado = deputado.IdEstado,
                    });
                }
                else
                    registrosAtualizados++;

                dbContext.SaveChanges();
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
            var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == config.Estado.ToString());
            if (importacao == null)
            {
                importacao = new Importacao()
                {
                    Chave = config.Estado.ToString()
                };
                dbContext.Importacoes.Add(importacao);
            }

            if (pInicio != null)
            {
                importacao.ParlamentarInicio = pInicio.Value;
                importacao.ParlamentarFim = null;
            }
            if (pFim != null) importacao.ParlamentarFim = pFim.Value;

            dbContext.SaveChanges();
        }
    }

    public class ImportadorParlamentarConfig
    {
        public string BaseAddress { get; set; }

        public Estado Estado { get; set; }

    }
}
