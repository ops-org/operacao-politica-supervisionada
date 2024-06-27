using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado de Minas Gerais
/// https://dadosabertos.almg.gov.br/
/// </summary>
public class MinasGerais : ImportadorBase
{
    public MinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarMinasGerais(serviceProvider);
        importadorDespesas = new ImportadorDespesasMinasGerais(serviceProvider);
    }
}

public class ImportadorDespesasMinasGerais : ImportadorDespesasRestApiAnual
{
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
    /// https://dadosabertos.almg.gov.br/ws/prestacao_contas/verbas_indenizatorias/ajuda
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ano"></param>
    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        using (var db = new AppDb())
        {
            var dc = db.ExecuteDict($"select id, matricula, nome_parlamentar from cl_deputado where id_estado = {idEstado}");
            foreach (var item in dc)
            {
                var matricula = item["matricula"].ToString();
                if (string.IsNullOrEmpty(matricula)) continue;

                Thread.Sleep(TimeSpan.FromSeconds(1));
                var doc = new XmlDocument();
                doc.Load($"{config.BaseAddress}ws/prestacao_contas/verbas_indenizatorias/legislatura_atual/deputados/{matricula}/datas");
                XmlNode deputados = doc.DocumentElement;

                var datas = deputados.SelectNodes("fechamentoVerba");

                foreach (XmlNode data in datas)
                {
                    var dataReferencia = Convert.ToDateTime(data.SelectSingleNode("dataReferencia ").InnerText);
                    if (dataReferencia.Year != ano) continue;

                    var docInner = new XmlDocument();
                    try
                    {
                        docInner.Load($"{config.BaseAddress}ws/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/{dataReferencia.Year}/{dataReferencia.Month}");
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                            throw;

                        Thread.Sleep(1000);
                        docInner.Load($"{config.BaseAddress}ws/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/{dataReferencia.Year}/{dataReferencia.Month}");
                    }

                    var despesas = docInner.DocumentElement.SelectNodes("resumoVerba/listaDetalheVerba/detalheVerba");

                    var sqlFields = new StringBuilder();
                    var sqlValues = new StringBuilder();

                    foreach (XmlNode despesa in despesas)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Ano = (short)dataReferencia.Year,
                            Documento = despesa.SelectSingleNode("descDocumento")?.InnerText,
                            DataEmissao = Convert.ToDateTime(despesa.SelectSingleNode("dataEmissao").InnerText),
                            Cpf = matricula,
                            Nome = item["nome_parlamentar"].ToString().ToTitleCase(),
                            TipoDespesa = despesa.SelectSingleNode("descTipoDespesa").InnerText,
                            CnpjCpf = despesa.SelectSingleNode("cpfCnpj")?.InnerText,
                            Empresa = despesa.SelectSingleNode("nomeEmitente").InnerText,
                            Valor = Convert.ToDecimal(despesa.SelectSingleNode("valorReembolsado").InnerText, cultureInfo)
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }

        }
    }
}

public class ImportadorParlamentarMinasGerais : ImportadorParlamentarBase
{

    public ImportadorParlamentarMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // https://dadosabertos.almg.gov.br/ws/ajuda/sobre

        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "http://dadosabertos.almg.gov.br/",
            Estado = Estado.MinasGerais,
        });
    }

    public override Task Importar()
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        foreach (var situacao in new[] { "em_exercicio", "que_exerceram_mandato", "que_renunciaram", "que_se_afastaram", "que_perderam_mandato" })
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(@$"{config.BaseAddress}ws/deputados/{situacao}");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                    throw;

                Thread.Sleep(1000);
                doc.Load(@$"{config.BaseAddress}ws/deputados/{situacao}");
            }

            var deputadoXml = doc.DocumentElement.SelectNodes("deputado");
            foreach (XmlNode fileNode in deputadoXml)
            {
                var matricula = Convert.ToUInt32(fileNode.SelectSingleNode("id").InnerText);
                var deputado = GetDeputadoByMatriculaOrNew(matricula);

                deputado.IdPartido = BuscarIdPartido(fileNode.SelectSingleNode("partido").InnerText);
                deputado.NomeParlamentar = fileNode.SelectSingleNode("nome").InnerText.ToTitleCase();
                deputado.UrlPerfil = $"https://www.almg.gov.br/deputados/conheca_deputados/deputados-info.html?idDep={deputado.Matricula}&leg=20";

                Thread.Sleep(TimeSpan.FromSeconds(1));

                var docPerfil = new XmlDocument();
                docPerfil.Load(@$"{config.BaseAddress}ws/deputados/{deputado.Matricula}");
                XmlNode detalhes = docPerfil.DocumentElement;

                deputado.NomeCivil = detalhes.SelectSingleNode("nomeServidor").InnerText.ToTitleCase();
                deputado.Naturalidade = detalhes.SelectSingleNode("naturalidadeMunicipio").InnerText;
                if (detalhes.SelectSingleNode("dataNascimento") != null)
                    deputado.Nascimento = DateOnly.Parse(detalhes.SelectSingleNode("dataNascimento").InnerText, cultureInfo);
                deputado.Profissao = detalhes.SelectSingleNode("atividadeProfissional")?.InnerText.ToTitleCase();
                deputado.Sexo = detalhes.SelectSingleNode("sexo").InnerText;

                InsertOrUpdate(deputado);
            }
        }

        return Task.CompletedTask;
    }
}

