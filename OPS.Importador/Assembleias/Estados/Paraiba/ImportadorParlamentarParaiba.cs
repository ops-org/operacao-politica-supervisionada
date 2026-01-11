using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Paraiba
{
    public class ImportadorParlamentarParaiba : ImportadorParlamentarRestApi
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarConfig()
            {
                BaseAddress = "https://sapl3.al.pb.leg.br/",
                Estado = Estado.Paraiba,
            });
        }

        public override Task Importar()
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var legislatura = 20; // 2023-2027
            var address = $"{config.BaseAddress}api/parlamentares/legislatura/{legislatura}/parlamentares/?get_all=true";
            List<Congressman> parlamentares = RestApiGet<List<Congressman>>(address);

            foreach (var parlamentar in parlamentares)
            {
                var matricula = (int)parlamentar.Id;
                DeputadoEstadual deputado = GetDeputadoByNameOrNew(parlamentar.NomeParlamentar);

                deputado.UrlPerfil = $"https://sapl3.al.pb.leg.br/parlamentar/{parlamentar.Id}";
                deputado.NomeParlamentar = parlamentar.NomeParlamentar.ToTitleCase();
                deputado.IdPartido = BuscarIdPartido(parlamentar.Partido);
                deputado.UrlFoto = parlamentar.Fotografia;

                ObterDetalhesDoPerfil(deputado).GetAwaiter().GetResult();

                InsertOrUpdate(deputado);
            }

            logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
            return Task.CompletedTask;
        }

        private async Task ObterDetalhesDoPerfil(DeputadoEstadual deputado)
        {
            var context = httpClient.CreateAngleSharpContext();

            var document = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
            if (document.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
            }
            ;

            var perfil = document.QuerySelector("#content");

            deputado.NomeCivil = perfil.QuerySelector("#div_nome").TextContent.Split(":")[1].Trim().ToTitleCase();

            var elementos = perfil.QuerySelectorAll(".form-group>p").Select(x => x.TextContent);
            if (elementos.Any())
            {
                deputado.Email = elementos.Where(x => x.StartsWith("E-mail")).FirstOrDefault()?.Split(':')[1].Trim().NullIfEmpty();
                deputado.Telefone = elementos.Where(x => x.StartsWith("Telefone")).FirstOrDefault()?.Split(':')[1].Trim().NullIfEmpty();
            }
            else
            {
                logger.LogWarning("Verificar possivel mudança no perfil do parlamentar: {UrlPerfil}", deputado.UrlPerfil);
            }
        }

        private class Congressman
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("nome_parlamentar")]
            public string NomeParlamentar { get; set; }

            [JsonPropertyName("fotografia_cropped")]
            public string FotografiaCropped { get; set; }

            [JsonPropertyName("fotografia")]
            public string Fotografia { get; set; }

            [JsonPropertyName("ativo")]
            public bool Ativo { get; set; }

            [JsonPropertyName("partido")]
            public string Partido { get; set; }

            [JsonPropertyName("titular")]
            public string Titular { get; set; }
        }
    }
}
