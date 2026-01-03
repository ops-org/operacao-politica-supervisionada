using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AngleSharp;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul
{
    public class ImportadorDespesasRioGrandeDoSul : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfoBR = CultureInfo.CreateSpecificCulture("pt-BR");
        private CultureInfo cultureInfoUS = CultureInfo.CreateSpecificCulture("en-US");

        public ImportadorDespesasRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://transparencia.al.rs.gov.br",
                Estado = Estado.RioGrandeDoSul,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar // O Gabinete muda a cada legislatura
            };
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano)
        {
            var tasks = new List<Task>();

            tasks.Add(ImportarDespesasAno(context, ano));
            tasks.Add(ImportarDiariasAno(context, ano));

            Task.WaitAll(tasks.ToArray());
        }

        private async Task ImportarDespesasAno(IBrowsingContext context, int ano)
        {
            var urlParlamentaresListarMes = $"{config.BaseAddress}/ajax-gastosParlamentaresListarMes?ano={ano}";
            ParlamentaresListarMes objParlamentaresListarMes;
            try
            {
                objParlamentaresListarMes = await RestApiGetAsync<ParlamentaresListarMes>(urlParlamentaresListarMes);
                if (objParlamentaresListarMes.Lista is null)
                {
                    logger.LogWarning("Dados para o ano {Ano} ainda não disponiveis!", ano);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dados para o ano {Ano} ainda não disponiveis!", ano);
                return;
            }

            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            var meses = objParlamentaresListarMes.Lista.Select(x => x.Mes);
            Parallel.ForEach(meses, parallelOptions, mes =>
            {
                //if (ano == 2019 && mes == 1) continue;
                if (ano == DateTime.Now.Year && mes > DateTime.Today.Month) return;

                using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes }))
                {

                    try
                    {
                        ImportarDespesasMes(context, ano, mes).Wait();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                }
            });
        }

        private async Task ImportarDespesasMes(IBrowsingContext context, int ano, int mes)
        {

            var urlParlamentaresListarGabinete = $"{config.BaseAddress}/ajax-gastosParlamentaresListarGabinete?ano={ano}&mes={mes}";
            ParlamentaresListarGabinete objParlamentaresListarGabinete = await RestApiGetAsync<ParlamentaresListarGabinete>(urlParlamentaresListarGabinete);

            foreach (var gabinete in objParlamentaresListarGabinete.Lista)
            {
                var urlDespesas = $"{config.BaseAddress}/parlamentares/gastos/pesquisa?ano={ano}&mes={mes}&solicitante={gabinete.Codigo}";
                var document = await context.OpenAsyncAutoRetry(urlDespesas);

                var elementoDespesasDoMes = document
                    .QuerySelectorAll(".contratos-ativos__header-item--contratada")
                    .FirstOrDefault(x => x.TextContent.Trim().Equals("4- Despesas do Mês"));

                if (elementoDespesasDoMes is null)
                {
                    logger.LogWarning("Dados não disponiveis para o parlamentar {Parlamentar} para {Mes:00}/{Ano}.", gabinete.Parlamentar, mes, ano);
                    continue;
                }

                var matrizDespesas = elementoDespesasDoMes
                    .Closest(".contratos-ativos__row")
                    .QuerySelectorAll(".contratos-ativos__contrato>.contratos-ativos__conteudo>.remuneracao__nome");

                foreach (var despesa in matrizDespesas)
                {
                    var despesaUnica = despesa.QuerySelectorAll(".responsive-value");
                    if (despesaUnica[0].TextContent.StartsWith("Total")) continue;

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = gabinete.Parlamentar.Replace("Gabinete Dep.", "").Replace("55", "").Trim(),
                        Cpf = gabinete.Codigo.ToString(),
                        Ano = (short)ano,
                        Mes = (short)mes,
                        TipoDespesa = despesaUnica[0].TextContent.Replace(":", ""),
                        Valor = Convert.ToDecimal(despesaUnica[1].TextContent.Replace(" - R$", ""), cultureInfoBR),
                        DataEmissao = new DateTime(ano, mes, 1)
                    };

                    if (despesaTemp.Valor == 0) continue;

                    InserirDespesaTemp(despesaTemp);
                }

                var elementoOutrosCreditos = document
                    .QuerySelectorAll(".contratos-ativos__contratada-mobile")
                    .FirstOrDefault(x => x.TextContent.StartsWith("Outros Créditos"));

                if (elementoOutrosCreditos != null)
                {
                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = gabinete.Parlamentar.Replace("Gabinete Dep.", "").Replace("55", "").Trim(),
                        Cpf = gabinete.Codigo.ToString(),
                        Ano = (short)ano,
                        Mes = (short)mes,
                        TipoDespesa = "Outros Créditos",
                        Valor = -Convert.ToDecimal(elementoOutrosCreditos.TextContent.Split("R$")[1].Trim(), cultureInfoBR),
                        DataEmissao = new DateTime(ano, mes, 1)
                    };

                    if (despesaTemp.Valor == 0) continue;

                    InserirDespesaTemp(despesaTemp);
                }
            }
        }

        private async Task ImportarDiariasAno(IBrowsingContext context, int ano)
        {
            var urlDiarias = $"{config.BaseAddress}/pessoal/diarias/deputados/pesquisa?anoDiaria={ano}&tipoDiaria=todos&solicitante=";
            var document = await context.OpenAsyncAutoRetry(urlDiarias);

            var deputados = document.QuerySelectorAll(".contratos-ativos a.contratos-ativos__objeto-modal.span-ajax");
            foreach (var deputado in deputados)
            {
                if (deputado.Attributes["data-codproponente"] == null) continue;
                var nomeDeputado = deputado.TextContent.Trim().ToTitleCase();
                if (nomeDeputado == "Detalhes") continue;

                var parameters = new Dictionary<string, string>()
            {
                { "codProponente", deputado.Attributes["data-codproponente"].Value},
                { "anoDiaria", ano.ToString()},
                { "tipoDiaria", "todos"},
            };
                var urlDiariasRS = $"{config.BaseAddress}/ajax-DiariasModal";
                DiariasRS objDiariasRS = await RestApiPostAsync<DiariasRS>(urlDiariasRS, parameters);

                foreach (var diaria in objDiariasRS.Lista)
                {
                    var dataDiaria = Convert.ToDateTime(diaria.DataDiaria);
                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        NomeCivil = nomeDeputado,
                        Ano = (short)ano,
                        Mes = (short)diaria.Mes,
                        TipoVerba = "Diárias",
                        TipoDespesa = diaria.TipoDiaria.DisplayName() ?? diaria.IdTipoDiaria.ToString(),
                        Valor = Convert.ToDecimal(diaria.TotalPago, cultureInfoUS),
                        DataEmissao = dataDiaria,
                        Documento = diaria.IdRequisicaoDiaria.ToString(),
                        Observacao = $"Destino: {diaria.Destino.Trim()}; Ida: {diaria.Ida.Split(" ")[0]}; Volta: {diaria.Volta.Split(" ")[0]}; Diárias: {diaria.Diarias}"
                    };

                    InserirDespesaTemp(despesaTemp);

                    parameters = new Dictionary<string, string>()
                {
                    { "id", diaria.IdRequisicaoDiaria.ToString()},
                    { "dataInicio", diaria.Ida},
                    { "dataFim", diaria.Volta},
                };
                    var urlDiariaDetalhesRS = $"{config.BaseAddress}/ajax-DiariasModal2";
                    DiariaDetalhesRS objDiariaDetalhesRS = await RestApiPostAsync<DiariaDetalhesRS>(urlDiariaDetalhesRS, parameters);

                    foreach (var bilhete in objDiariaDetalhesRS.Bilhetes)
                    {
                        var observacao = new StringBuilder();
                        observacao.Append($"Destino: {bilhete.Destino.Trim()}; Localizador: {bilhete.Localizador}; ");
                        if (bilhete.DataSaida != null)
                            observacao.Append($"Ida: {bilhete.DataSaida}; ");
                        if (bilhete.DataRetorno != null)
                            observacao.Append($"Volta: {bilhete.DataRetorno}; ");

                        var despesaBilheteTemp = new CamaraEstadualDespesaTemp()
                        {
                            NomeCivil = nomeDeputado,
                            Ano = (short)ano,
                            Mes = (short)diaria.Mes,
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(bilhete.CgcCompanhiaAerea),
                            Empresa = bilhete.NomeCompanhia.Trim(),
                            TipoVerba = "Diárias",
                            TipoDespesa = diaria.TipoDiaria.DisplayName() ?? diaria.IdTipoDiaria.ToString(),
                            Valor = 0,
                            DataEmissao = Convert.ToDateTime(bilhete.DataSaida ?? bilhete.DataRetorno),
                            Documento = bilhete.IdRequisicaoDiaria.ToString(),
                            Observacao = observacao.ToString().Trim()
                        };

                        InserirDespesaTemp(despesaBilheteTemp);
                    }

                    foreach (var nota in objDiariaDetalhesRS.Notasfiscais)
                    {
                        var despesaNotaFiscalTemp = new CamaraEstadualDespesaTemp()
                        {
                            NomeCivil = nomeDeputado,
                            Ano = (short)ano,
                            Mes = (short)diaria.Mes,
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(nota.CnpjEstabelecimento),
                            Empresa = nota.NomeEstabelecimento.Trim(),
                            TipoVerba = "Diárias",
                            TipoDespesa = diaria.TipoDiaria.DisplayName() ?? diaria.IdTipoDiaria.ToString(),
                            Valor = Convert.ToDecimal(nota.ValorTotal, cultureInfoUS),
                            DataEmissao = Convert.ToDateTime(nota.DataEmissao),
                            Documento = nota.IdRequisicaoDiaria.ToString(),
                            Observacao = $"Municipio: {nota.Municipio.Trim()}; Num Nota: {nota.NumNota}"
                        };

                        InserirDespesaTemp(despesaNotaFiscalTemp);
                    }

                    JsonSerializerOptions options = new()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    };
                    //if (objDiariaDetalhesRS.Atestados.Any()) Console.WriteLine("Atestados: " + JsonSerializer.Serialize(objDiariaDetalhesRS.Atestados, options));
                    //if (objDiariaDetalhesRS.Certificados.Any()) Console.WriteLine("Certificados: " + JsonSerializer.Serialize(objDiariaDetalhesRS.Certificados, options));
                    if (objDiariaDetalhesRS.Transportes.Any()) Console.WriteLine("Transportes: " + JsonSerializer.Serialize(objDiariaDetalhesRS.Transportes, options));
                    if (objDiariaDetalhesRS.Relatorios.Any()) Console.WriteLine("Relatorios: " + JsonSerializer.Serialize(objDiariaDetalhesRS.Relatorios, options));
                }
            }
        }

        public override void AjustarDados()
        {
            connection.Execute($@"
UPDATE ops_tmp.cl_despesa_temp temp
JOIN cl_deputado d ON d.nome_civil = temp.nome_civil
SET temp.nome = d.nome_parlamentar
WHERE despesa_tipo = 'Diárias'
and d.id_estado = {idEstado}
");
        }
    }
}
