using System.Xml;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Assembleias.SaoPaulo
{
    public class ImportadorParlamentarSaoPaulo : ImportadorParlamentarBase
    {
        public ImportadorParlamentarSaoPaulo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarConfig()
            {
                BaseAddress = "https://www.al.sp.gov.br/",
                Estado = Estados.SaoPaulo,
            });
        }

        public override Task Importar()
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var doc = new XmlDocument();
            doc.Load($"{config.BaseAddress}repositorioDados/deputados/deputados.xml");
            XmlNodeList deputadoXml = doc.DocumentElement.SelectNodes("Deputado");

            foreach (XmlNode fileNode in deputadoXml)
            {
                var matricula = Convert.ToInt32(fileNode.SelectSingleNode("Matricula").InnerText.Trim());
                var deputado = GetDeputadoByMatriculaOrNew(matricula);

                deputado.NomeParlamentar = fileNode.SelectSingleNode("NomeParlamentar").InnerText.Trim().ToTitleCase();
                deputado.IdPartido = BuscarIdPartido(fileNode.SelectSingleNode("Partido").InnerText.Trim());

                deputado.Telefone = fileNode.SelectSingleNode("Telefone")?.InnerText.Trim();
                deputado.Email = fileNode.SelectSingleNode("Email")?.InnerText?.Trim();
                //var idDeputado = fileNode.SelectSingleNode("IdDeputado").InnerText.Trim();
                //var gabinete = fileNode.SelectSingleNode("Sala").InnerText.Trim();
                if (fileNode.SelectSingleNode("PathFoto") != null)
                    deputado.UrlFoto = fileNode.SelectSingleNode("PathFoto").InnerText.Trim();
                else
                    deputado.UrlFoto = $"https://legis-backend-api-portal.pub.al.sp.gov.br/api_portal/biografia/obter-foto/{matricula}";

                deputado.UrlPerfil = $"{config.BaseAddress}deputado/?matricula={deputado.Matricula}";

                if (string.IsNullOrEmpty(deputado.NomeCivil))
                    deputado.NomeCivil = deputado.NomeParlamentar;

                InsertOrUpdate(deputado);

            }

            logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
            return Task.CompletedTask;
        }
    }
}
