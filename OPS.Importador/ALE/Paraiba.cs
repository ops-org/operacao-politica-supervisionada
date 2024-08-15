using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
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

            using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = gabinete.Text, ["Url"] = linkPlanilha, ["Arquivo"] = Path.GetFileName(caminhoArquivo) }))
            {
                if (TentarBaixarArquivo(linkPlanilha, caminhoArquivo))
                {
                    try
                    {
                        ImportarDespesas(caminhoArquivo, ano, mes, gabinete.Value, gabinete.Text);
                    }
                    catch (Exception)
                    {

                        //logger.LogError(ex, ex.Message);

#if !DEBUG
                        //Excluir o arquivo para tentar importar novamente na proxima execução
                        if(System.IO.File.Exists(caminhoArquivo))
                            System.IO.File.Delete(caminhoArquivo);
#endif

                    }
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
                if (valorTotalDeputado != valorTotalArquivo)
                    logger.LogError("Valor Divergente! Esperado: {ValorTotalArquivo}; Encontrado: {ValorTotalDeputado}; Diferenca: {Diferenca}",
                        valorTotalArquivo, valorTotalDeputado, valorTotalArquivo - valorTotalDeputado);

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

            if (!string.IsNullOrEmpty(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode())))
                despesaTemp.DataEmissao = Convert.ToDateTime(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode()), cultureInfo);
            else
                despesaTemp.DataEmissao = new DateTime(ano, mes, 1);

            InserirDespesaTemp(despesaTemp);
            valorTotalDeputado += despesaTemp.Valor;
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


public class ImportadorParlamentarParaiba : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "http://sapl.al.pb.leg.br/sapl/consultas/parlamentar/parlamentar_index_html?hdn_num_legislatura=19",
            SeletorListaParlamentares = ".tileItem",
            Estado = Estado.Paraiba,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeCivil = item.QuerySelector(".tileHeadline a").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByFullNameOrNew(nomeCivil);

        deputado.UrlPerfil = (item.QuerySelector(".tileHeadline a") as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".parlamentar-partido .texto").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.NomeParlamentar = subDocument.QuerySelector("h1.firstHeading").TextContent.Trim().ToTitleCase();
        deputado.UrlFoto = (subDocument.QuerySelector("img.parlamentar") as IHtmlImageElement)?.Source;

        var perfil = subDocument.QuerySelectorAll("#texto-parlamentar b")
            .Select(x => new { Key = x.TextContent.Replace(":", "").Trim(), Value = x.NextSibling.TextContent.Trim() });

        if (!string.IsNullOrEmpty(perfil.First(x => x.Key == "Data Nascimento")?.Value))
            deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Data Nascimento").Value, cultureInfo);

        deputado.Email = perfil.FirstOrDefault(x => x.Key == "E-mail")?.Value.NullIfEmpty();
        deputado.Telefone = perfil.FirstOrDefault(x => x.Key == "Telefone")?.Value.NullIfEmpty();

        // ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".deputado ul a")); // Todos são as redes sociaos da AL
    }
}