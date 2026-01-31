using System.Globalization;
using System.Text;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.SantaCatarina
{
    public class ImportadorDespesasSantaCatarina : ImportadorDespesasArquivo
    {
        public ImportadorDespesasSantaCatarina(IServiceProvider serviceProvider) : base(serviceProvider)
        {

            base.config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://sapl.al.ac.leg.br/parlamentar/",
                Estado = Estados.SantaCatarina,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }


        /// <summary>
        /// Dados a partir de 2011
        /// https://transparencia.alesc.sc.gov.br/gabinetes_dados_abertos.php
        /// </summary>
        /// <param name="ano"></param>
        /// <returns></returns>
        public override Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();

            // https://transparencia.alesc.sc.gov.br/gabinetes_dados_abertos.php
            // Arquivos disponiveis anualmente a partir de 2011
            var urlOrigem = $"https://transparencia.alesc.sc.gov.br/gabinetes_csv.php?ano={ano}";
            var caminhoArquivo = Path.Combine(tempFolder, $"CLSC-{ano}.csv");

            //if (DateTime.Now.AddMonths(-1).Year >= ano && File.Exists(caminhoArquivo)) File.Delete(caminhoArquivo);

            arquivos.Add(urlOrigem, caminhoArquivo);
            return arquivos;
        }

        public override void ImportarDespesas(string caminhoArquivo, int ano)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

            int indice = 0;
            int Verba = indice++;
            int Descricao = indice++;
            int Conta = indice++;
            int Favorecido = indice++;
            int Trecho = indice++;
            int Vencimento = indice++;
            int Valor = indice++;

            using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
            using (var csv = new CsvReader(reader, cultureInfo))
            {
                var linhasProcessadasAno = 0;
                while (csv.Read())
                {
                    linhasProcessadasAno++;

                    if (linhasProcessadasAno == 1)
                    {

                        if (
                            csv[Verba] != "Verba" ||
                            csv[Descricao] != "Descrição" ||
                            csv[Conta] != "Conta" ||
                            csv[Favorecido] != "Favorecido" ||
                            csv[Trecho] != "Trecho" ||
                            csv[Vencimento] != "Vencimento" ||
                            csv[Valor] != "Valor"
                        )
                            throw new Exception("Mudança de integração detectada para o Câmara Legislativa de Santa Catarina");

                        // Pular linha de titulo
                        continue;
                    }

                    if (string.IsNullOrEmpty(csv[Verba])) continue; //Linha vazia

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = csv[Conta].Split("(")[0].Trim().ToTitleCase(),
                        Ano = (short)ano,
                        TipoVerba = csv[Verba],
                        TipoDespesa = csv[Descricao],
                        Valor = Convert.ToDecimal(csv[Valor], cultureInfo),
                        DataEmissao = DateOnly.Parse(csv[Vencimento]),
                        Favorecido = csv[Favorecido],
                        Observacao = csv[Trecho],
                        Origem = caminhoArquivo,
                    };

                    // Nome da empresa vem no favorecido para despesas que não sõa de diárias e passagens.
                    if(despesaTemp.TipoVerba != "DIÁRIAS" && despesaTemp.TipoVerba != "PASSAGENS")
                    {
                        despesaTemp.NomeFornecedor = despesaTemp.Favorecido;
                        despesaTemp.Favorecido = null;
                    }

                    InserirDespesaTemp(despesaTemp);
                }

                // Ignorar linha de titulo
                logger.LogDebug("{Linhas} linhas processadas!", --linhasProcessadasAno);
            }
        }

        public override void AtualizaParlamentarValores()
        {
            connection.Execute(@"
        	        UPDATE cl_deputado dp SET
                        valor_total_ceap = coalesce((
                            SELECT SUM(ds.valor_liquido) FROM assembleias.cl_despesa ds WHERE ds.id_cl_deputado = dp.id
                        ), 0);");
        }

        public override void AjustarDados()
        {
            connection.Execute(@"
UPDATE temp.cl_despesa_temp SET id_fornecedor = 89481 WHERE id_fornecedor IS NULL AND fornecedor ILIKE 'Brasil Telecom%';
UPDATE temp.cl_despesa_temp SET id_fornecedor = 458 WHERE id_fornecedor IS NULL AND (fornecedor ILIKE 'Oi S%' OR fornecedor ILIKE 'Oi Fixo%' OR fornecedor ILIKE 'Oi');
UPDATE temp.cl_despesa_temp SET id_fornecedor = 301 WHERE id_fornecedor IS NULL AND (fornecedor ILIKE 'Global Village Telecom%' OR fornecedor ILIKE 'GVT');
UPDATE temp.cl_despesa_temp SET id_fornecedor = 8688 WHERE id_fornecedor IS NULL AND fornecedor ILIKE 'Claro';
UPDATE temp.cl_despesa_temp SET id_fornecedor = 4163 WHERE id_fornecedor IS NULL AND fornecedor ILIKE 'NET';

UPDATE temp.cl_despesa_temp SET id_fornecedor = 19411 WHERE id_fornecedor IS NULL AND lower(unaccent(despesa_tipo)) =  lower(unaccent('Restaurante da AFALESC'));
UPDATE temp.cl_despesa_temp SET id_fornecedor = 663 WHERE id_fornecedor IS NULL AND (lower(unaccent(despesa_tipo)) = lower(unaccent('Energia Elétrica ( Escritório de Apoio )')) or lower(unaccent(despesa_tipo)) = lower(unaccent('Escritório de Apoio - Energia Elétrica')));
UPDATE temp.cl_despesa_temp SET id_fornecedor = 1316 WHERE id_fornecedor IS NULL and (lower(unaccent(despesa_tipo)) = lower(unaccent('Água (Escritório de Apoio)')) OR lower(unaccent(despesa_tipo)) = lower(unaccent('Escritório de Apoio - Água')));
UPDATE temp.cl_despesa_temp SET id_fornecedor = 1163 WHERE id_fornecedor IS NULL and lower(unaccent(despesa_tipo)) = lower(unaccent('Assinatura de TV a Cabo'));
UPDATE temp.cl_despesa_temp SET id_fornecedor = 42634 WHERE id_fornecedor IS NULL and lower(unaccent(despesa_tipo)) = lower(unaccent('Correspondência / Telegrama'));
UPDATE temp.cl_despesa_temp SET id_fornecedor = 47839 WHERE id_fornecedor IS NULL and lower(unaccent(despesa_tipo)) = lower(unaccent('Locação de Veículo (Contrato)'));
");
        }
    }
}
