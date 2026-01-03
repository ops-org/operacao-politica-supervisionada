using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro
{
    public class ImportadorParlamentarRioDeJaneiro : ImportadorParlamentarRestApi
    {

        public ImportadorParlamentarRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarConfig()
            {
                BaseAddress = "https://docigp.alerj.rj.gov.br/api/v1/",
                Estado = Estado.RioDeJaneiro,
            });
        }

        public override Task Importar()
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"withMandate\":false,\"withoutMandate\":false,\"withPendency\":false,\"withoutPendency\":false,\"unread\":false,\"joined\":true,\"notJoined\":false,\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":83,\"per_page\":\"250\",\"current_page\":1,\"last_page\":9,\"from\":1,\"to\":10,\"pages\":[1,2,3,4,5]}}";
            var address = $"{config.BaseAddress}congressmen?query=" + WebUtility.UrlEncode(query);
            Congressman objDeputadosRJ = RestApiGet<Congressman>(address);

            foreach (var parlamentar in objDeputadosRJ.Data)
            {
                var matricula = (uint)parlamentar.Id;
                DeputadoEstadual deputado = GetDeputadoByMatriculaOrNew(matricula);

                if (parlamentar.RemoteId != null)
                    deputado.UrlPerfil = $"https://www.alerj.rj.gov.br/Deputados/PerfilDeputado/{parlamentar.RemoteId}?Legislatura=20";

                if (parlamentar.Party != null)
                    deputado.IdPartido = BuscarIdPartido(parlamentar.Party.Code);

                deputado.NomeParlamentar = parlamentar.Nickname.ToTitleCase();
                deputado.NomeCivil = parlamentar.Name.ToTitleCase();
                deputado.Email = parlamentar.User.Email;
                //deputado.Telefone = parlamentar.TelefoneDeputado;
                deputado.UrlFoto = parlamentar.PhotoUrl;

                InsertOrUpdate(deputado);
            }

            logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
            return Task.CompletedTask;
        }
    }
}
