using System.Globalization;
using AngleSharp;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Estados.Pernambuco.Entities;

namespace OPS.Importador.Assembleias.Estados.Pernambuco;

public class ImportadorDespesasPernambuco : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasPernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.alepe.pe.gov.br/servicos/transparencia/",
            Estado = Estado.Pernambuco,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        List<RubricasPE> rubricas;
        //try
        //{
        //    rubricas = RestApiGetWithCustomDateConverter<List<RubricasPE>>($"{config.BaseAddress}adm/verbaindenizatoria-rubricas.php?ano={ano}");
        //}
        //catch (Exception)
        //{
        rubricas = new List<RubricasPE>
            {
                new RubricasPE(){ NumeroCategoria = "1", NomeCategoria = "IMÓVEIS"},
                new RubricasPE(){ NumeroCategoria = "2", NomeCategoria = "Locação de veículos"},
                new RubricasPE(){ NumeroCategoria = "3", NomeCategoria = "Assessoria Jurídica"},
                new RubricasPE(){ NumeroCategoria = "4", NomeCategoria = "CONSULTORIAS E TRABALHOS TÉCNICOS"},
                new RubricasPE(){ NumeroCategoria = "5", NomeCategoria = "Divulgação da atividade parlamentar"},
                new RubricasPE(){ NumeroCategoria = "6", NomeCategoria = "SERVIÇOS DE TELECOMUNICAÇÕES EM GERAL"},
                new RubricasPE(){ NumeroCategoria = "7", NomeCategoria = "SERVIÇOS E PRODUTOS POSTAIS"},
                new RubricasPE(){ NumeroCategoria = "9", NomeCategoria = "FORNECIMENTO DE ALIMENTAÇÃO DO PARLAMENTAR"},
                new RubricasPE(){ NumeroCategoria = "10", NomeCategoria = "SERVIÇOS DE SEGURANÇA"},
            };
        //}


        var deputados = RestApiGetWithCustomDateConverter<List<DeputadoPE>>($"{config.BaseAddress}dep/deputados.php?leg=-16"); // 2023-2026
        if (!deputados.Any())
        {
            logger.LogWarning("Nenhum parlamentar foi encontrado.");
            return;
        }

        foreach (var deputado in deputados)
        {
            var meses = RestApiGetWithCustomDateConverter<List<DespesaMesesPE>>($"{config.BaseAddress}adm/verbaindenizatoria-dep-meses.php?dep={deputado.Id}&ano={ano}");
            foreach (var mesComDespesa in meses)
            {
                var documentos = RestApiGetWithCustomDateConverter<List<DespesaDocumentosPE>>($"{config.BaseAddress}adm/verbaindenizatoria.php?dep={deputado.Id}&ano={ano}&mes={mesComDespesa.Mes}");
                if (!documentos.Any())
                {
                    logger.LogWarning("Não da documentos para o parlamentar {Parlamentar} em {Mes}/{Ano}", deputado.Nome, mesComDespesa.Mes, ano);
                    continue;
                }

                foreach (var documento in documentos)
                {
                    var despesas = RestApiGetWithCustomDateConverter<List<DespesaPE>>($"{config.BaseAddress}adm/verbaindenizatorianotas.php?docid={documento.Docid}");
                    if (!despesas.Any())
                    {
                        logger.LogWarning("Não da despesas para o parlamentar {Parlamentar} em {Mes}/{Ano}", deputado.Nome, mesComDespesa.Mes, ano);
                        continue;
                    }

                    foreach (var despesa in despesas)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = deputado.Nome,
                            Cpf = deputado.Id,
                            Ano = (short)ano,
                            Mes = Convert.ToInt16(mesComDespesa.Mes),
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(despesa.Cnpj),
                            Empresa = despesa.Empresa,
                            Documento = documento.Docid,
                            Valor = Convert.ToDecimal(despesa.Valor, cultureInfo),
                            DataEmissao = Convert.ToDateTime(despesa.Data, cultureInfo),
                            TipoDespesa = rubricas.First(x => x.NumeroCategoria == despesa.Rubrica).NomeCategoria,
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }
        }
    }

    public override void AjustarDados()
    {
        connection.Execute(@"
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '11364583000156' WHERE cnpj_cpf = '11136458300015';
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '02421421000111' WHERE cnpj_cpf = '02421421001255';
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '18376563000306' WHERE cnpj_cpf = '18376663000306';
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '18376563000306' WHERE cnpj_cpf = '18376563000366';

");
    }
}
