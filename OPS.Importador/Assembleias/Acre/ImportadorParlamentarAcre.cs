using System.Net;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Acre.Entities;
using OPS.Importador.Comum.Parlamentar;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Acre
{
    public class ImportadorParlamentarAcre : ImportadorParlamentarRestApi
    {

        public ImportadorParlamentarAcre(IServiceProvider serviceProvider) : base(serviceProvider)
        {

            Configure(new ImportadorParlamentarConfig()
            {
                BaseAddress = "https://sapl.al.ac.leg.br/",
                Estado = Estados.Acre,
            });
        }

        public override Task Importar()
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var legislatura = 17; // 2023-2027
            var address = $"{config.BaseAddress}api/parlamentares/legislatura/{legislatura}/parlamentares/?get_all=true";
            List<DeputadoAcre> objDeputadosAcre = RestApiGet<List<DeputadoAcre>>(address);

            foreach (var parlamentar in objDeputadosAcre)
            {
                var matricula = (int)parlamentar.Id;
                DeputadoEstadual deputado = GetDeputadoByMatriculaOrNew(matricula);

                deputado.UrlPerfil = $"https://sapl.al.ac.leg.br/parlamentar/{parlamentar.Id}";
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
                deputado.Email = elementos.Where(x => x.StartsWith("E-mail")).FirstOrDefault()?.Split(':')[1].Trim();
                deputado.Telefone = elementos.Where(x => x.StartsWith("Telefone")).FirstOrDefault()?.Split(':')[1].Trim().NullIfEmpty();
            }
            else
            {
                logger.LogWarning("Verificar possivel mudança no perfil do parlamentar: {UrlPerfil}", deputado.UrlPerfil);
            }
        }
    }
}
