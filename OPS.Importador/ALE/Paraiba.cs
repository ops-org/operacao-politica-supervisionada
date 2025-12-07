using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.ALE.Comum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE;

public class Paraiba : ImportadorBase
{
    public Paraiba(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarParaiba(serviceProvider);
        importadorDespesas = new ImportadorDespesasParaiba(serviceProvider);
    }
}

public class ImportadorDespesasParaiba : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "http://www.al.pb.leg.br/transparencia/",
            Estado = Estado.Paraiba,
            ChaveImportacao = ChaveDespesaTemp.Gabinete
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}deputados/viap-v2?tipo_viap=deputados&ano_viap={ano}&mes_viap={mes}";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("#content form");

        var gabinetes = (form.QuerySelector("select[name=deputado]") as IHtmlSelectElement).Options;
        foreach (var gabinete in gabinetes)
        {
            if (gabinete.Value == "0") continue;

            var dcForm = new Dictionary<string, string>();
            dcForm.Add("deputado", gabinete.Value);
            var subDocument = form.SubmitAsyncAutoRetry(dcForm, true).GetAwaiter().GetResult();

            var linkPlanilha = (subDocument.QuerySelector("#content ul.lista-v a") as IHtmlAnchorElement).Href;
            var caminhoArquivo = $"{tempPath}/CLPB-{ano}-{mes}-{gabinete.Value}.ods";

            // Parse the URL and get the query string
            Uri uri = new Uri(linkPlanilha);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            // Get the 'src' parameter (which is URL encoded)
            string linkPlanilhaLimpa = queryParams["src"];

            using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = gabinete.Text, ["Url"] = linkPlanilha, ["Arquivo"] = Path.GetFileName(caminhoArquivo) }))
            {
                BaixarArquivo(linkPlanilhaLimpa, caminhoArquivo);

                try
                {
                    ImportarDespesas(caminhoArquivo, ano, mes, gabinete.Value, gabinete.Text);
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, ex.Message);

#if !DEBUG
                    //Excluir o arquivo para tentar importar novamente na proxima execução
                    if(System.IO.File.Exists(caminhoArquivo))
                        System.IO.File.Delete(caminhoArquivo);
#endif

                }
            }
        }
    }

    public void ImportarDespesas(string file, int ano, int mes, string gabinete, string nomeParlamentar)
    {

        //Prima di tutto ci serve una istanza 
        //  dell'oggetto RedOdsReader
        RedOdsReader OdsObj = new RedOdsReader();

        //Apriamo un file .ODS
        OdsObj.LoadFile(file);

        //impostiamo il nome del foglio/tabella da leggere
        var sheetName = "Plan1";

        decimal valorTotalDeputado = 0;
        int despesasIncluidas = 0;
        var linha = 7;
        while (true)
        {
            linha++;
            if (linha > 1000) //string.IsNullOrEmpty(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Competencia.GetHashCode())))
            {
                logger.LogError("Valor Não validado: {ValorTotal}; Referencia: {Mes}/{Ano}; Parlamentar: {Parlamentar}", valorTotalDeputado, mes, ano, nomeParlamentar);
                return;
            }
            if (OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Numero.GetHashCode()).StartsWith("Total de Despesas"))
            {
                var valorTotalArquivo = Convert.ToDecimal(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Valor.GetHashCode()), cultureInfo);
                ValidaValorTotal(valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

                return;
            }
            if (string.IsNullOrEmpty(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Item.GetHashCode()))) continue;

            var despesaTemp = new CamaraEstadualDespesaTemp()
            {
                Nome = nomeParlamentar,
                Cpf = gabinete,
                Ano = (short)ano,
                Mes = (short)mes,
                TipoVerba = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Item.GetHashCode()),
                TipoDespesa = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.SubItem.GetHashCode()),
                CnpjCpf = Utils.RemoveCaracteresNaoNumericos(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.CnpjCpf.GetHashCode())),
                Empresa = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Fornecedor.GetHashCode()),
                Documento = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Numero.GetHashCode()),
                Observacao = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Documento.GetHashCode()),
                Valor = Convert.ToDecimal(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Valor.GetHashCode()), cultureInfo),

            };

            var dataEmissao = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode());
            if (!string.IsNullOrEmpty(dataEmissao) && dataEmissao != "#######")
                despesaTemp.DataEmissao = Convert.ToDateTime(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode()), cultureInfo);
            else
                despesaTemp.DataEmissao = new DateTime(ano, mes, 1);

            InserirDespesaTemp(despesaTemp);
            valorTotalDeputado += despesaTemp.Valor;
            despesasIncluidas++;
        }


    }

    public override void AjustarDados()
    {
        // Atualizar numero do gabinete no perfil do parlamentar
        connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp dt
JOIN cl_deputado d ON d.nome_parlamentar = dt.nome AND d.id_estado = 25
SET d.gabinete = dt.cpf
WHERE d.gabinete IS null;

UPDATE ops_tmp.cl_despesa_temp dt
JOIN cl_deputado d ON d.nome_civil = dt.nome AND d.id_estado = 25
SET d.gabinete = dt.cpf
WHERE d.gabinete IS NULL;");

        // Desanonimizar os CNPJs/CPFs dentro do possivel
        connection.Execute(@"
INSERT IGNORE INTO ops_tmp.fornecedor_correcao (cnpj_cpf, nome)
SELECT distinct cnpj_cpf, empresa 
FROM ops_tmp.cl_despesa_temp;

UPDATE ops_tmp.fornecedor_correcao d
JOIN fornecedor_info f ON f.nome = d.nome AND f.cnpj LIKE CONCAT('___', d.cnpj_cpf, '___')
SET d.cnpj_cpf_correto = f.cnpj
WHERE LENGTH(d.cnpj_cpf) = 8
AND f.tipo = 'MATRIZ'
AND d.cnpj_cpf_correto IS NULL;

UPDATE ops_tmp.cl_despesa_temp d
JOIN ops_tmp.fornecedor_correcao c ON c.nome = d.empresa AND c.cnpj_cpf = d.cnpj_cpf
SET d.cnpj_cpf = c.cnpj_cpf_correto
WHERE c.cnpj_cpf_correto IS NOT null;

UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = CONCAT('***', d.cnpj_cpf, '***') WHERE LENGTH(d.cnpj_cpf) = 5;
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = CONCAT('***', d.cnpj_cpf, '***') WHERE LENGTH(d.cnpj_cpf) = 8;
                ");


        var cnpjInvalidos = connection.ExecuteScalar<int>(@"select count(1) from ops_tmp.cl_despesa_temp where LENGTH(cnpj_cpf) != 11 && LENGTH(cnpj_cpf) != 14");
        if (cnpjInvalidos > 0)
        {
            throw new BusinessException("Há CPNJs/CPFs invalidos que devem ser corrigidos manualmente! #2");
            // SELECT DISTINCT d.empresa, d.cnpj_cpf FROM ops_tmp.cl_despesa_temp d WHERE LENGTH(d.cnpj_cpf) < 10
        }
    }


    private enum ColunasOds
    {
        Competencia = 1,
        Deputado,
        Item,
        SubItem,
        Fornecedor,
        CnpjCpf,
        Data,
        Documento,
        Numero,
        Valor
    }
}


public class ImportadorParlamentarParaiba : ImportadorParlamentarRestApi
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "https://sapl3.al.pb.leg.br/",
            Estado = Estado.Paraiba,
        });
    }

    public override Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var legislatura = 20; // 2023-2027
        var address = $"{config.BaseAddress}api/parlamentares/legislatura/{legislatura}/parlamentares/?get_all=true";
        List<Congressman> parlamentares = RestApiGet<List<Congressman>>(address);

        foreach (var parlamentar in parlamentares)
        {
            var matricula = (uint)parlamentar.Id;
            DeputadoEstadual deputado = GetDeputadoByNameOrNew(parlamentar.NomeParlamentar);

            deputado.UrlPerfil = $"https://sapl3.al.pb.leg.br/parlamentar/{parlamentar.Id}";
            deputado.NomeParlamentar = parlamentar.NomeParlamentar.ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(parlamentar.Partido);
            deputado.UrlFoto = parlamentar.Fotografia;

            ObterDetalhesDoPerfil(deputado).GetAwaiter().GetResult();

            InsertOrUpdate(deputado);
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        return Task.CompletedTask;
    }

    private async Task ObterDetalhesDoPerfil(DeputadoEstadual deputado)
    {
        var context = httpClient.CreateAngleSharpContext();

        var document = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
        if (document.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        };

        var perfil = document.QuerySelector("#content");

        deputado.NomeCivil = perfil.QuerySelector("#div_nome").TextContent.Split(":")[1].Trim().ToTitleCase();

        var elementos = perfil.QuerySelectorAll(".form-group>p").Select(x => x.TextContent);
        if (elementos.Any())
        {
            deputado.Email = elementos.Where(x => x.StartsWith("E-mail")).FirstOrDefault()?.Split(':')[1].Trim().NullIfEmpty();
            deputado.Telefone = elementos.Where(x => x.StartsWith("Telefone")).FirstOrDefault()?.Split(':')[1].Trim().NullIfEmpty();
        }
        else
        {
            logger.LogWarning("Verificar possivel mudança no perfil do parlamentar: {UrlPerfil}", deputado.UrlPerfil);
        }
    }

    private class Congressman
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("fotografia_cropped")]
        public string FotografiaCropped { get; set; }

        [JsonPropertyName("fotografia")]
        public string Fotografia { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("titular")]
        public string Titular { get; set; }
    }
}


