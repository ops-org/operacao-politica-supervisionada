using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.RioGrandeDoSul.Entities;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Assembleias.RioGrandeDoSul
{
    public class ImportadorParlamentarRioGrandeDoSul : ImportadorParlamentarRestApi
    {
        public ImportadorParlamentarRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarConfig()
            {
                BaseAddress = "https://ww4.al.rs.gov.br:5000/",
                Estado = Estados.RioGrandeDoSul,
            });
        }

        public override Task Importar()
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var address = $"{config.BaseAddress}listarDestaqueDeputados";
            DeputadosRS objDeputadosRS = RestApiGet<DeputadosRS>(address);

            foreach (var parlamentar in objDeputadosRS.Lista)
            {
                var matricula = (int)parlamentar.IdDeputado;
                var deputado = GetDeputadoByMatriculaOrNew(matricula);

                deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
                deputado.NomeParlamentar = parlamentar.NomeDeputado.Trim().ReduceWhitespace().ToTitleCase();
                deputado.IdPartido = BuscarIdPartido(parlamentar.SiglaPartido);
                deputado.Email = parlamentar.EmailDeputado.NullIfEmpty();
                deputado.Telefone = parlamentar.TelefoneDeputado;
                deputado.UrlFoto = parlamentar.FotoGrandeDeputado;

                if (string.IsNullOrEmpty(deputado.NomeCivil))
                    deputado.NomeCivil = deputado.NomeParlamentar;

                InsertOrUpdate(deputado);
            }

            logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
            return Task.CompletedTask;
        }
    }
}
