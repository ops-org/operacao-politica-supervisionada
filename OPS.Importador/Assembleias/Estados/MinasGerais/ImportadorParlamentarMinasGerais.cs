using System.Globalization;
using Microsoft.Extensions.Logging;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Estados.MinasGerais.Entities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.MinasGerais;

public class ImportadorParlamentarMinasGerais : ImportadorParlamentarBase
{

    public ImportadorParlamentarMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // https://dadosabertos.almg.gov.br/api/v2ajuda/sobre

        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "http://dadosabertos.almg.gov.br/",
            Estado = Estado.MinasGerais,
        });
    }

    public override Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        // 1=em exercício, 2=exerceu mandato, 3=renunciou, 4=afastado, 5=perdeu mandato
        foreach (var situacao in new[] { 1, 2, 3, 4, 5 })
        {
            DeputadoListMG deputados;
            try
            {
                deputados = RestApiGetWithSqlTimestampConverter<DeputadoListMG>(@$"{config.BaseAddress}api/v2/deputados/situacao/{situacao}?formato=json");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                    throw;

                Thread.Sleep(1000);
                deputados = RestApiGetWithSqlTimestampConverter<DeputadoListMG>(@$"{config.BaseAddress}api/v2/deputados/situacao/{situacao}?formato=json");
            }

            foreach (DeputadoMG deputado in deputados.List)
            {
                var matricula = deputado.Id;
                var deputadoDb = GetDeputadoByMatriculaOrNew((int)matricula);

                deputadoDb.IdPartido = BuscarIdPartido(deputado.Partido);
                deputadoDb.NomeParlamentar = deputado.Nome.ToTitleCase();
                deputadoDb.UrlPerfil = $"https://www.almg.gov.br/deputados/conheca_deputados/deputados-info.html?idDep={deputadoDb.Matricula}&leg=20";

                //Thread.Sleep(TimeSpan.FromSeconds(1));
                var detalhes = RestApiGetWithSqlTimestampConverter<DeputadoDetalhesMG>(@$"{config.BaseAddress}api/v2/deputados/{deputadoDb.Matricula}?formato=json");

                deputadoDb.NomeCivil = detalhes.Deputado.NomeServidor.ToTitleCase();
                deputadoDb.Naturalidade = detalhes.Deputado.NaturalidadeMunicipio;
                if (detalhes.Deputado.DataNascimento != null)
                    deputadoDb.Nascimento = DateOnly.Parse(detalhes.Deputado.DataNascimento, cultureInfo);
                deputadoDb.Profissao = detalhes.Deputado.AtividadeProfissional.ToTitleCase();
                deputadoDb.Sexo = detalhes.Deputado.Sexo;

                InsertOrUpdate(deputadoDb);
            }
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", registrosInseridos, registrosAtualizados);
        return Task.CompletedTask;
    }
}