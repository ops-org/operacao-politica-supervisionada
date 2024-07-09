using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
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
            var subDocument = form.SubmitAsync(dcForm, true).GetAwaiter().GetResult();

            var linkPlanilha = (subDocument.QuerySelector("#content ul.lista-v a") as IHtmlAnchorElement).Href;
            var caminhoArquivo = $"{tempPath}/CLPB-{ano}-{mes}.ods";
            if (TentarBaixarArquivo(linkPlanilha, caminhoArquivo))
            {
                try
                {
                    ImportarDespesas(caminhoArquivo, ano, mes, gabinete.Value, gabinete.Text);
                }
                catch (Exception ex)
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

    public void ImportarDespesas(string file, int ano, int mes, string gabinete, string nomeParlamentar)
    {

        //Prima di tutto ci serve una istanza 
        //  dell'oggetto RedOdsReader
        RedOdsReader OdsObj = new RedOdsReader();

        //Apriamo un file .ODS
        OdsObj.LoadFile(file);

        //impostiamo il nome del foglio/tabella da leggere
        var sheetName = "Plan1";

        var linha = 8;
        while (true)
        {
            if (OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Numero.GetHashCode()).StartsWith("Total de Despesas)")) break;

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
                DataEmissao = Convert.ToDateTime(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode()), cultureInfo)
            };

            InserirDespesaTemp(despesaTemp);
            linha++;
        }
    }

    public override void AjustarDados()
    {

        connection.Execute(@"
UPDATE ops_tmp.cl_despesa_temp dt
JOIN cl_deputado d ON d.nome_parlamentar = dt.nome AND d.id_estado = 25
SET d.gabinete = dt.cpf
WHERE d.gabinete IS null;

UPDATE ops_tmp.cl_despesa_temp dt
JOIN cl_deputado d ON d.nome_civil = dt.nome AND d.id_estado = 25
SET d.gabinete = dt.cpf
WHERE d.gabinete IS NULL;

UPDATE ops_tmp.cl_despesa_temp d
JOIN fornecedor f ON f.nome = d.empresa AND f.cnpj_cpf LIKE CONCAT('%', d.cnpj_cpf, '%')
SET d.cnpj_cpf = f.cnpj_cpf
WHERE LENGTH(d.cnpj_cpf) = 8;

UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '10541486000129' WHERE d.cnpj_cpf =  '41486000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '22608457000116' WHERE d.cnpj_cpf =  '08457000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '30408699000194' WHERE d.cnpj_cpf =  '08699000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '08697211000137' WHERE d.cnpj_cpf =  '97211000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '05854086000133' WHERE d.cnpj_cpf =  '54086000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '11372084001778' WHERE d.cnpj_cpf =  '72084001';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '34368160000100' WHERE d.cnpj_cpf =  '68160000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '01300648000146' WHERE d.cnpj_cpf =  '00648000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '70098470000115' WHERE d.cnpj_cpf =  '98470000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '09352634000188' WHERE d.cnpj_cpf =  '52634000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '03882108000143' WHERE d.cnpj_cpf =  '82108000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '11521613000190' WHERE d.cnpj_cpf =  '21613000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '06916962000252' WHERE d.cnpj_cpf =  '16962000';
UPDATE ops_tmp.cl_despesa_temp d SET d.cnpj_cpf = '***221264**' WHERE d.cnpj_cpf =  '22126';
                ");


        var cnpjInvalidos = connection.ExecuteScalar<int>(@"select count(1) from ops_tmp.cl_despesa_temp where LENGTH(cnpj_cpf) < 10");
        if(cnpjInvalidos > 0)
        {
            throw new BusinessException("Há CPNJs/CPFs invalidos que devem ser corrigidos manualmente!");
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