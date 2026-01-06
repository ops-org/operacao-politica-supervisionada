using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Estados.MinasGerais.Entities;

namespace OPS.Importador.Assembleias.Estados.MinasGerais;

public class ImportadorDespesasMinasGerais : ImportadorDespesasArquivo
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://dadosabertos.almg.gov.br/",
            Estado = Estado.MinasGerais,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };
    }

    /// <summary>
    /// https://dadosabertos.almg.gov.br/api/ajuda/swagger/view/lastest#/
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ano"></param>
    public new Task Importar(int ano)
    {
        using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
        {
            //// TODO: Criar importação por legislatura
            //if (ano != 2023)
            //{
            //    logger.LogWarning("Importação já realizada para o ano de {Ano}", ano);
            //    //throw new BusinessException($"Importação já realizada para o ano de {ano}");
            //}

            CarregarHashes(ano);

            var dc = connection.Query<Dictionary<string, object>>($"select id, matricula, nome_parlamentar from cl_deputado where id_estado = {idEstado}").ToList();
            foreach (var item in dc)
            {
                var matricula = item["matricula"].ToString();
                if (string.IsNullOrEmpty(matricula)) continue;

                var address = $"{config.BaseAddress}api/v2/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/datas?formato=json";
                ListaFechamentoVerbaDatas resDiarias = RestApiGet<ListaFechamentoVerbaDatas>(address);

                foreach (ListaFechamentoVerba data in resDiarias.ListaFechamentoVerba)
                {
                    var dataReferencia = Convert.ToDateTime(data.DataReferencia);
                    //if (dataReferencia.Year < ano) continue; // TODO: Ajustar para importar do mandato
                    if (dataReferencia.Year != ano) continue; // TODO: Ajustar para importar do mandato

                    ListaMensalDespesasMG despesasMensais;
                    address = $"{config.BaseAddress}api/v2/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/{dataReferencia.Year}/{dataReferencia.Month}?formato=json";
                    try
                    {
                        despesasMensais = RestApiGetWithSqlTimestampConverter<ListaMensalDespesasMG>(address);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                            throw;

                        Thread.Sleep(1000);
                        despesasMensais = RestApiGetWithSqlTimestampConverter<ListaMensalDespesasMG>(address);
                    }

                    var despesas = despesasMensais.List.SelectMany(x => x.ListaDetalheVerba);

                    var sqlFields = new StringBuilder();
                    var sqlValues = new StringBuilder();

                    foreach (ListaDetalheVerba despesa in despesas)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Ano = (short)dataReferencia.Year,
                            Mes = (short)dataReferencia.Month,
                            Documento = despesa.DescDocumento,
                            DataEmissao = despesa.DataEmissao,
                            Cpf = matricula,
                            Nome = item["nome_parlamentar"].ToString().ToTitleCase(),
                            TipoDespesa = despesa.DescTipoDespesa,
                            CnpjCpf = despesa.CpfCnpj,
                            Empresa = despesa.NomeEmitente,
                            Valor = Convert.ToDecimal(despesa.ValorReembolsado, cultureInfo)
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }

            ProcessarDespesas(ano);
        }

        return Task.CompletedTask;
    }

    //public override void DefinirCompetencias(int ano)
    //{
    //    competenciaInicial = $"{ano}01";
    //    competenciaFinal = $"{ano + 4}12";
    //}

    public override void ImportarDespesas(string caminhoArquivo, int ano)
    {
        throw new NotImplementedException();
    }
}