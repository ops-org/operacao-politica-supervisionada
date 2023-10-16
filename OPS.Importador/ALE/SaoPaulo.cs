using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE
{
    /// <summary>
    /// Assembleia Legislativa do Estado de São Paulo
    /// https://www.al.sp.gov.br/transparencia/
    /// https://www.al.sp.gov.br/dados-abertos/
    /// </summary>
    public class SaoPaulo : ImportadorBase
    {
        public SaoPaulo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarSaoPaulo(serviceProvider);
            importadorDespesas = new ImportadorDespesasSaoPaulo(serviceProvider);
        }
    }

    public class ImportadorDespesasSaoPaulo : ImportadorDespesasArquivo
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        /// <summary>
        /// Dados a partir de 2002
        /// https://www.al.sp.gov.br/deputado/contas/
        /// https://www.al.sp.gov.br/dados-abertos/recurso/21
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ImportadorDespesasSaoPaulo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://www.al.sp.gov.br/",
                Estado = Estado.SaoPaulo,
                ChaveImportacao = ChaveDespesaTemp.Matricula
            };
        }

        public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();
            string urlOrigem, caminhoArquivo;

            urlOrigem = $"{config.BaseAddress}repositorioDados/deputados/despesas_gabinetes_{ano}.xml";
            caminhoArquivo = $"{tempPath}/CLSP-{ano}.xml";

            arquivos.Add(urlOrigem, caminhoArquivo);
            return arquivos;
        }

        public override void ImportarDespesas(string caminhoArquivo, int ano)
        {
            var doc = new XmlDocument();
            doc.Load(caminhoArquivo);
            var despesas = doc.DocumentElement.SelectNodes("despesa");

            foreach (XmlNode fileNode in despesas)
            {
                var matricula = fileNode.SelectSingleNode("Matricula").InnerText.Trim();
                var deputado = fileNode.SelectSingleNode("Deputado").InnerText.Trim().ToTitleCase();
                var tipoDespesa = fileNode.SelectSingleNode("Tipo").InnerText.Trim();
                //var ano = fileNode.SelectSingleNode("Ano").InnerText.Trim();
                var mes = fileNode.SelectSingleNode("Mes").InnerText.Trim();
                var cnpj = fileNode.SelectSingleNode("CNPJ")?.InnerText?.Trim();
                var fornecedor = fileNode.SelectSingleNode("Fornecedor").InnerText.Trim();
                var valor = fileNode.SelectSingleNode("Valor").InnerText.Trim();

                var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = deputado,
                    Ano = (short)ano,
                    Cpf = matricula,
                    Empresa = fornecedor,
                    CnpjCpf = cnpj,
                    TipoDespesa = tipoDespesa,
                    Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor, cultureInfo) : 0,
                    DataEmissao = new DateTime(ano, Convert.ToInt32(mes), 1)
                };

                InserirDespesaTemp(objCamaraEstadualDespesaTemp);
            }
        }

        public override void AjustarDados()
        {
            connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '00000000000008' WHERE empresa = 'PEDÁGIO';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '00000000000009' WHERE empresa = 'TAXI';
UPDATE ops_tmp.cl_despesa_temp SET cnpj_cpf = '04645433000155' WHERE empresa = 'ACACIA PRANDI ZANIN';
        ");
        }
    }

    public class ImportadorParlamentarSaoPaulo : ImportadorParlamentarBase
    {
        public ImportadorParlamentarSaoPaulo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarConfig()
            {
                BaseAddress = "https://www.al.sp.gov.br/",
                Estado = Estado.SaoPaulo,
            });
        }

        public override Task Importar()
        {
            logger.LogWarning("Parlamentares do(a) {idEstado}:{CasaLegislativa}", config.Estado.GetHashCode(), config.Estado.ToString());
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var doc = new XmlDocument();
            doc.Load($"{config.BaseAddress}repositorioDados/deputados/deputados.xml");
            XmlNodeList deputadoXml = doc.DocumentElement.SelectNodes("Deputado");

            foreach (XmlNode fileNode in deputadoXml)
            {
                var matricula = Convert.ToUInt32(fileNode.SelectSingleNode("Matricula").InnerText.Trim());
                var deputado = GetDeputadoByMatriculaOrNew(matricula);

                deputado.NomeParlamentar = fileNode.SelectSingleNode("NomeParlamentar").InnerText.Trim().ToTitleCase();
                deputado.IdPartido = BuscarIdPartido(fileNode.SelectSingleNode("Partido").InnerText.Trim());

                deputado.Telefone = fileNode.SelectSingleNode("Telefone")?.InnerText.Trim();
                deputado.Email = fileNode.SelectSingleNode("Email")?.InnerText?.Trim();
                //var idDeputado = fileNode.SelectSingleNode("IdDeputado").InnerText.Trim();
                //var gabinete = fileNode.SelectSingleNode("Sala").InnerText.Trim();
                deputado.UrlFoto = fileNode.SelectSingleNode("PathFoto").InnerText.Trim();
                deputado.UrlPerfil = $"{config.BaseAddress}deputado/?matricula={deputado.Matricula}";

                InsertOrUpdate(deputado);

            }

            logger.LogWarning("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
            return Task.CompletedTask;
        }
    }
}
