using AngleSharp;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Comum.Despesa
{
    public abstract class ImportadorDespesasRestApiAnual : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiAnual(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task Importar(int ano, CancellationToken ct = default)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                CarregarHashes(ano);

                var context = httpClientResilient.CreateAngleSharpContext();

                await ImportarDespesas(context, ano, ct);
                ProcessarDespesas(ano);
            }
        }

        public abstract Task ImportarDespesas(IBrowsingContext context, int ano, CancellationToken ct = default);
    }

    //public class ImportadorDespesasRestApiConfig
    //{
    //    public string BaseAddress { get; internal set; }
    //    public Estado Estado { get; internal set; }
    //}
}
