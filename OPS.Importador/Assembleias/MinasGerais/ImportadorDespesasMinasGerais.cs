using System.Globalization;
using System.Text;
using OPS.Core.Enumerators;
using OPS.Importador.Assembleias.MinasGerais.Entities;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.MinasGerais;

public class ImportadorDespesasMinasGerais : ImportadorDespesasArquivo
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://dadosabertos.almg.gov.br/",
            Estado = Estados.MinasGerais,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };
    }

    /// <summary>
    /// https://dadosabertos.almg.gov.br/api/ajuda/swagger/view/lastest#/
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ano"></param>
    public override async Task Importar(int ano)
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

            var deputados = dbContext.DeputadosEstaduais.Where(x => x.IdEstado == idEstado).ToList();
            foreach (var deputado in deputados)
            {
                if (deputado.Matricula == null) continue;

                var address = $"{config.BaseAddress}api/v2/prestacao_contas/verbas_indenizatorias/deputados/{deputado.Matricula}/datas?formato=json";
                ListaFechamentoVerbaDatas resDiarias = await RestApiGet<ListaFechamentoVerbaDatas>(address);

                foreach (ListaFechamentoVerba data in resDiarias.ListaFechamentoVerba)
                {
                    var dataReferencia = Convert.ToDateTime(data.DataReferencia);
                    //if (dataReferencia.Year < ano) continue; // TODO: Ajustar para importar do mandato
                    if (dataReferencia.Year != ano) continue; // TODO: Ajustar para importar do mandato

                    ListaMensalDespesasMG despesasMensais;
                    address = $"{config.BaseAddress}api/v2/prestacao_contas/verbas_indenizatorias/deputados/{deputado.Matricula}/{dataReferencia.Year}/{dataReferencia.Month}?formato=json";
                    try
                    {
                        despesasMensais = await RestApiGetWithSqlTimestampConverter<ListaMensalDespesasMG>(address);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                            throw;

                        Thread.Sleep(1000);
                        despesasMensais = await RestApiGetWithSqlTimestampConverter<ListaMensalDespesasMG>(address);
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
                            DataEmissao = DateOnly.FromDateTime(despesa.DataEmissao),
                            Cpf = deputado.Matricula.ToString(),
                            Nome = deputado.NomeParlamentar,
                            TipoDespesa = despesa.DescTipoDespesa,
                            CnpjCpf = despesa.CpfCnpj,
                            NomeFornecedor = despesa.NomeEmitente,
                            Valor = Convert.ToDecimal(despesa.ValorReembolsado, cultureInfo),
                            Origem = address
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }

            ProcessarDespesas(ano);
        }
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