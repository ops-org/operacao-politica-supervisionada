using System.Globalization;
using System.Text;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OPS.Core.Enumerators;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Paraiba
{
    public class ImportadorDespesasParaiba : ImportadorDespesasRestApiMensal
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "http://www.al.pb.leg.br/transparencia/",
                Estado = Estados.Paraiba,
                ChaveImportacao = ChaveDespesaTemp.Gabinete
            };
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano, int mes)
        {
            var address = $"{config.BaseAddress}deputados/viap-v2?tipo_viap=deputados&ano_viap={ano}&mes_viap={mes}";
            var document = await context.OpenAsyncAutoRetry(address);

            IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("#content form");

            var gabinetes = (form.QuerySelector("select[name=deputado]") as IHtmlSelectElement).Options;
            foreach (var gabinete in gabinetes)
            {
                if (gabinete.Value == "0") continue;

                var dcForm = new Dictionary<string, string>();
                dcForm.Add("deputado", gabinete.Value);
                var subDocument = await form.SubmitAsyncAutoRetry(dcForm, true);

                var linkPlanilha = (subDocument.QuerySelector("#content ul.lista-v a") as IHtmlAnchorElement).Href;

                // Parse the URL and get the query string
                Uri uri = new Uri(linkPlanilha);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);

                // Get the 'src' parameter (which is URL encoded)
                string linkPlanilhaLimpa = queryParams["src"];

                var extensao = Path.GetExtension(linkPlanilhaLimpa).ToLower();
                var caminhoArquivo = Path.Combine(tempFolder, $"CLPB-{ano}-{mes}-{gabinete.Value}{extensao}");

                using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = gabinete.Text, ["Url"] = linkPlanilhaLimpa, ["Arquivo"] = Path.GetFileName(caminhoArquivo) }))
                {
                    await fileManager.BaixarArquivo(dbContext, linkPlanilhaLimpa, caminhoArquivo, config.Estado);

                    try
                    {
                        if (extensao == ".ods")
                            ImportarDespesasOds(caminhoArquivo, ano, mes, gabinete.Value, gabinete.Text);
                        else if (extensao == ".xlsx")
                            ImportarDespesasXlsx(caminhoArquivo, ano, mes, gabinete.Value, gabinete.Text);
                        else
                            throw new BusinessException($"Extensão de arquivo não suportada: {extensao}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Erro ao importar despesas do parlamentar {Parlamentar} - Arquivo: {Arquivo}", gabinete.Text, Path.GetFileName(caminhoArquivo));
                        fileManager.MoverArquivoComErro(caminhoArquivo);
                    }
                }
            }
        }

        public void ImportarDespesasOds(string filename, int ano, int mes, string gabinete, string nomeParlamentar)
        {

            //Prima di tutto ci serve una istanza 
            //  dell'oggetto RedOdsReader
            RedOdsReader OdsObj = new RedOdsReader();

            //Apriamo un file .ODS
            OdsObj.LoadFile(filename);

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
                    ValidaValorTotal(filename, valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

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
                    Origem = filename
                };

                var dataEmissao = OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode());
                if (!string.IsNullOrEmpty(dataEmissao) && dataEmissao != "#######")
                    despesaTemp.DataEmissao = DateOnly.Parse(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Data.GetHashCode()), cultureInfo);
                else
                    despesaTemp.DataEmissao = new DateOnly(ano, mes, 1);

                InserirDespesaTemp(despesaTemp);
                valorTotalDeputado += despesaTemp.Valor;
                despesasIncluidas++;
            }
        }

        public void ImportarDespesasXlsx(string filename, int ano, int mes, string gabinete, string nomeParlamentar)
        {
            decimal valorTotalDeputado = 0;
            int despesasIncluidas = 0;

            using (var reader = new StreamReader(filename, Encoding.GetEncoding("UTF-8")))
            using (var package = new ExcelPackage(reader.BaseStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                for (int linha = 7; linha <= worksheet.Dimension.End.Row; linha++)
                {
                    if (linha > 1000) //string.IsNullOrEmpty(OdsObj.GetCellValueText(sheetName, linha, ColunasOds.Competencia.GetHashCode())))
                    {
                        logger.LogError("Valor Não validado: {ValorTotal}; Referencia: {Mes}/{Ano}; Parlamentar: {Parlamentar}", valorTotalDeputado, mes, ano, nomeParlamentar);
                        return;
                    }
                    if (worksheet.Cells[linha, ColunasOds.Numero.GetHashCode()].Value?.ToString()?.StartsWith("Total de Despesas") ?? false)
                    {
                        var valorTotalArquivo = Convert.ToDecimal(worksheet.Cells[linha, ColunasOds.Valor.GetHashCode()].Value, cultureInfo);
                        ValidaValorTotal(filename, valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

                        return;
                    }
                    if (string.IsNullOrEmpty(worksheet.Cells[linha, ColunasOds.Item.GetHashCode()].Value?.ToString())) continue;

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = nomeParlamentar,
                        Cpf = gabinete,
                        Ano = (short)ano,
                        Mes = (short)mes,
                        TipoVerba = worksheet.Cells[linha, ColunasOds.Item.GetHashCode()].Value?.ToString(),
                        TipoDespesa = worksheet.Cells[linha, ColunasOds.SubItem.GetHashCode()].Value?.ToString(),
                        CnpjCpf = Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[linha, ColunasOds.CnpjCpf.GetHashCode()].Value?.ToString()),
                        Empresa = worksheet.Cells[linha, ColunasOds.Fornecedor.GetHashCode()].Value?.ToString(),
                        Documento = worksheet.Cells[linha, ColunasOds.Numero.GetHashCode()].Value?.ToString(),
                        Observacao = worksheet.Cells[linha, ColunasOds.Documento.GetHashCode()].Value?.ToString(),
                        Valor = Convert.ToDecimal(worksheet.Cells[linha, ColunasOds.Valor.GetHashCode()].Value, cultureInfo),
                        Origem = filename
                    };

                    var dataEmissao = worksheet.Cells[linha, ColunasOds.Data.GetHashCode()].Value?.ToString();
                    if (!string.IsNullOrEmpty(dataEmissao) && dataEmissao != "#######")
                        despesaTemp.DataEmissao = DateOnly.Parse(dataEmissao, cultureInfo);
                    else
                        despesaTemp.DataEmissao = new DateOnly(ano, mes, 1);

                    InserirDespesaTemp(despesaTemp);
                    valorTotalDeputado += despesaTemp.Valor;
                    despesasIncluidas++;
                }
            }
        }

        public override void AjustarDados()
        {
            // Atualizar numero do gabinete no perfil do parlamentar
            connection.Execute($@"
UPDATE temp.cl_despesa_temp dt
SET cpf = d.gabinete
FROM assembleias.cl_deputado d
WHERE d.nome_parlamentar = dt.nome 
AND d.id_estado = {config.Estado.GetHashCode()}
AND d.gabinete IS null;

UPDATE temp.cl_despesa_temp dt
SET cpf = d.gabinete
FROM assembleias.cl_deputado d 
WHERE d.nome_civil = dt.nome
AND d.id_estado = {config.Estado.GetHashCode()}
AND d.gabinete IS NULL;");

            // Desanonimizar os CNPJs/CPFs dentro do possivel
            connection.Execute(@"
UPDATE temp.cl_despesa_temp SET cnpj_cpf = CONCAT('***', cnpj_cpf, '***') WHERE LENGTH(cnpj_cpf) = 5;
UPDATE temp.cl_despesa_temp SET cnpj_cpf = CONCAT('***', cnpj_cpf, '**') WHERE LENGTH(cnpj_cpf) = 6;
UPDATE temp.cl_despesa_temp SET cnpj_cpf = CONCAT('***', cnpj_cpf, '***') WHERE LENGTH(cnpj_cpf) = 8;

INSERT INTO temp.fornecedor_de_para (cnpj_incorreto, nome)
SELECT distinct cnpj_cpf, unaccent(lower(empresa))
FROM temp.cl_despesa_temp dt
LEFT JOIN temp.fornecedor_de_para fp on unaccent(lower(fp.nome)) = unaccent(lower(dt.empresa))
WHERE fp.cnpj_incorreto is null 
AND fp.nome is null;

UPDATE temp.fornecedor_de_para d
SET cnpj_correto = f.cnpj
FROM fornecedor.fornecedor_info f 
WHERE unaccent(lower(f.nome)) = unaccent(lower(d.nome)) 
AND f.cnpj ILIKE REPLACE(d.cnpj_incorreto, '*', '_')
AND f.tipo = 'MATRIZ'
AND d.cnpj_correto IS NULL;

UPDATE temp.cl_despesa_temp d
SET cnpj_cpf = c.cnpj_correto
FROM temp.fornecedor_de_para c 
WHERE c.nome = d.empresa 
AND c.cnpj_incorreto = d.cnpj_cpf
AND c.cnpj_correto IS NOT null;

UPDATE temp.cl_despesa_temp dt
SET cnpj_cpf = dp.cnpj_correto
FROM temp.fornecedor_de_para dp
WHERE dp.cnpj_incorreto = dt.cnpj_cpf
AND dp.cnpj_correto IS NOT NULL;");


            var cnpjInvalidos = connection.ExecuteScalar<int>(@"select count(1) from temp.cl_despesa_temp where LENGTH(cnpj_cpf) != 11 AND LENGTH(cnpj_cpf) != 14");
            if (cnpjInvalidos > 0)
            {
                throw new BusinessException("Há CPNJs/CPFs invalidos que devem ser corrigidos manualmente! #2");
                // SELECT DISTINCT d.empresa, d.cnpj_cpf FROM temp.cl_despesa_temp d WHERE LENGTH(d.cnpj_cpf) < 10
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
}
