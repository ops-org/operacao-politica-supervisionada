using System.Globalization;
using System.Text.Json;
using AngleSharp;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using RestSharp;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasParana(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://consultas.assembleia.pr.leg.br/api/",
                Estado = Estados.Parana,
                ChaveImportacao = ChaveDespesaTemp.Matricula
            };
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano)
        {
            // https://consultas.assembleia.pr.leg.br/#/ressarcimento
            await ImportarCotaParlamentar(ano);

            //OpenPageWaitForCaptchaAndClickConsultar(ano, mes);

            // TODO: Para importar as diarias precisamos primeiro importar a posição do parlamentar (ex: 4ª SECRETARIA) e o pessoal do gabinete;
            // http://transparencia.assembleia.pr.leg.br/pessoal/comissionados
            // http://transparencia.assembleia.pr.leg.br/pessoal/estaveis
            //ImportarDiarias(ano, mes);
        }

        private void ImportarDiarias(int ano, int mes)
        {
            var indice = 0;
            var idxPgto = indice++;
            var idxDataSaida = indice++;
            var idxDataRetorno = indice++;
            var idxProtocolo = indice++;
            var idxOrcamento = indice++;
            var idxEmitidoPara = indice++;
            var idxValor = indice++;
            var idxQuantidade = indice++;
            var idxMotivo = indice++;
            var idxDestino = indice++;

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());

            var address = $"http://transparencia.assembleia.pr.leg.br/api/diarias?ano={ano}&mes={mes}";
            var restClient = CreateHttpClient();

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            RestResponse resDiarias = restClient.Get(request);
            List<List<string>> diarias = JsonSerializer.Deserialize<List<List<string>>>(resDiarias.Content, options);

            foreach (var diaria in diarias)
            {
                if (diaria[idxEmitidoPara].StartsWith("GABINETE MILITAR")) continue;
                // TODO: Emitido para pode ser um comissionado e ainda não temos essa info.

                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = diaria[idxEmitidoPara].Split("-")[0].Trim().ToTitleCase(),
                    Ano = (short)ano,
                    Mes = (short)mes,
                    TipoDespesa = "Diárias",
                    Valor = Convert.ToDecimal(diaria[idxValor], cultureInfo),
                    DataEmissao = DateOnly.Parse(diaria[idxDataSaida], cultureInfo),
                    Documento = diaria[idxPgto],
                    Observacao = $"Trecho: {diaria[idxDestino]}; Protocolo: {diaria[idxProtocolo]} Orçamento: {diaria[idxOrcamento]}"
                };


                InserirDespesaTemp(despesaTemp);
            }
        }

        private Task ImportarCotaParlamentar(int ano)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());

            var files = Directory.EnumerateFiles(tempFolder, $"{ano}*");
            foreach (var file in files)
            {
                var filename = file.Replace("_tmp", "");
                _ = fileManager.ProcessarArquivoTemp(dbContext, filename, config.Estado);

                var content = File.ReadAllText(filename);
                if (content.Contains("invalid-recaptcha"))
                {
                    logger.LogError("Arquivo invalido: {CaminhoArquivo}", filename);
                    File.Delete(filename);
                    continue;
                }

                DespesasPR objDespesaPR = JsonSerializer.Deserialize<DespesasPR>(content, options);
                foreach (var parlamentarDespesa in objDespesaPR.Despesas)
                {
                    foreach (var despesa in parlamentarDespesa.DespesasAnuais?[0]?.DespesasMensais?[0]?.Despesas)
                    {
                        foreach (var itensDespesa in despesa.ItensDespesa)
                        {
                            var despesaTemp = new CamaraEstadualDespesaTemp()
                            {
                                Nome = parlamentarDespesa.Parlamentar.NomePolitico.Replace("DEPUTADA", "").Replace("DEPUTADO", "").Trim().ToTitleCase(),
                                Cpf = parlamentarDespesa.Parlamentar.Codigo.ToString(),
                                Ano = (short)itensDespesa.Exercicio,
                                TipoDespesa = itensDespesa.TipoDespesa.Descricao,
                                Valor = (decimal)(itensDespesa.Valor - itensDespesa.ValorDevolucao),
                                DataEmissao = DateOnly.FromDateTime(itensDespesa.Data),
                                CnpjCpf = itensDespesa.Fornecedor?.Documento,
                                NomeFornecedor = itensDespesa.Fornecedor?.Nome,
                                Documento = $"{itensDespesa.NumeroDocumento} [{itensDespesa.Codigo}/{itensDespesa.Numero}]",
                                Observacao = itensDespesa.Descricao,
                                Origem = filename
                            };

                            if (itensDespesa.Transporte != null)
                            {
                                var t = itensDespesa.Transporte;
                                var v = t.Veiculo;

                                despesaTemp.Observacao =
                                    $"{t.Descricao}; Veículo: {v.Placa}/{v.Modelo}; Distância: {t.Distancia:N0)}; Periodo: {t.DataSaida:dd/MM/yyyy} à {t.DataChegada:dd/MM/yyyy}";
                            }

                            if (itensDespesa.Diaria != null)
                            {
                                var d = itensDespesa.Diaria;

                                despesaTemp.Observacao =
                                    $"{d.Descricao}; Diárias: {d.NumeroDiarias:N1}; Região: {d.Regiao}";
                            }

                            InserirDespesaTemp(despesaTemp);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
