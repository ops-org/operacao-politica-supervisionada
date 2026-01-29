using System.Globalization;
using System.Xml;
using Dapper;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.SaoPaulo
{
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
                Estado = Estados.SaoPaulo,
                ChaveImportacao = ChaveDespesaTemp.Matricula
            };
        }

        public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();
            string urlOrigem, caminhoArquivo;

            urlOrigem = $"{config.BaseAddress}repositorioDados/deputados/despesas_gabinetes_{ano}.xml";
            caminhoArquivo = Path.Combine(tempFolder, $"CLSP-{ano}.xml");

            //if (DateTime.Now.AddMonths(-1).Year >= ano && File.Exists(caminhoArquivo)) File.Delete(caminhoArquivo);

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

                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = deputado,
                    Ano = (short)ano,
                    Cpf = matricula,
                    NomeFornecedor = fornecedor,
                    CnpjCpf = cnpj,
                    TipoDespesa = tipoDespesa,
                    Valor = !string.IsNullOrEmpty(valor) ? Convert.ToDecimal(valor, cultureInfo) : 0,
                    DataEmissao = new DateOnly(ano, Convert.ToInt32(mes), 1),
                    Origem = caminhoArquivo
                };


                if (despesaTemp.CnpjCpf?.StartsWith("00000000000") ?? false)
                {
                    // Parece que o CNPJ é obrigatório, e com isso cadastram CNPJs iniciais para empresas internacionais.
                    // Vamos 'zerar' o CNPJ para resolver a empresa apenas pelo nome.
                    despesaTemp.CnpjCpf = null;
                }

                InserirDespesaTemp(despesaTemp);
            }
        }
    }
}
