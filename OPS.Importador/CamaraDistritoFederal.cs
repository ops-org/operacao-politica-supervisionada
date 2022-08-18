using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OPS.Importador
{
    public class CamaraDistritoFederal : ImportadorCotaParlamentarBase
    {
        public CamaraDistritoFederal(ILogger<CamaraSantaCatarina> logger, IConfiguration configuration)
            : base("Câmara Legislativa do Distrito Federal", logger, configuration)
        {

        }

        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();
            string urlOrigem, caminhoArquivo;

            if (ano >= 2020)
            {
                urlOrigem = string.Format("https://dadosabertos.cl.df.gov.br/View/opendata/verbas/{0}_verba_indenizatoria.xlsx", ano);
                caminhoArquivo = $"{tempPath}/CLDF-{ano}.xlsx";

                arquivos.Add(urlOrigem, caminhoArquivo);
            }
            else if (ano == 2019)
            {
                CultureInfo usEnglish = new CultureInfo("pt-BR");


                for (int mes = 1; mes <= 12; mes++)
                {
                    string nomeMes = usEnglish.DateTimeFormat.MonthNames[mes - 1];

                    // Janeiro e Fevereiro usa apenas o prefixo do mês
                    if (ano == 2019 && mes <= 2)
                        nomeMes = nomeMes.Substring(0, 3);

                    urlOrigem = string.Format("https://dadosabertos.cl.df.gov.br/View/opendata/verbas/{0}_{1}_verba%20indenizatoria.xlsx", ano, nomeMes.ToUpper());
                    caminhoArquivo = $"{tempPath}/CLDF-{ano}-{mes}.xlsx";

                    arquivos.Add(urlOrigem, caminhoArquivo);
                }
            }
            else
            {
                if (ano == 2018)
                {
                    urlOrigem = string.Format("https://dadosabertos.cl.df.gov.br/View/opendata/verbas/{0}%20completa%20verba_indenizatoria.csv", ano);
                }
                else // if (ano <= 2017)
                {
                    urlOrigem = string.Format("https://dadosabertos.cl.df.gov.br/View/opendata/verbas/verba_indenizatoria_{0}.csv", ano);
                }

                caminhoArquivo = $"{tempPath}/CLDF-{ano}.csv";
                arquivos.Add(urlOrigem, caminhoArquivo);
            }

            return arquivos;
        }

        protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        {
            using (var banco = new AppDb())
            {
                LimpaDespesaTemporaria(banco);

                var dc = new Dictionary<string, UInt32>();
                using (var dReader = banco.ExecuteReader(
                    $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = 53 and d.ano_mes between {ano}01 and {ano}12"))
                    while (dReader.Read())
                        dc.Add(Convert.ToHexString((byte[])dReader["hash"]), (UInt32)dReader["id"]);

                foreach (var arquivo in arquivos)
                {
                    //var _urlOrigem = arquivo.Key;
                    caminhoArquivo = arquivo.Value;

                    if (caminhoArquivo.EndsWith(".xlsx"))
                        CarregaDadosXlsx(banco, caminhoArquivo, ano, dc);
                    else
                        CarregaDadosCsv(banco, caminhoArquivo, ano, dc);
                }

                foreach (var id in dc.Values)
                {
                    banco.AddParameter("id", id);
                    banco.ExecuteNonQuery("delete from cf_despesa where id=@id");
                }

                InsereTipoDespesaFaltante(banco);
                InsereDeputadoFaltante(banco);
                InsereFornecedorFaltante(banco);
                InsereDespesaFinal(banco);
                LimpaDespesaTemporaria(banco);

                if (ano == DateTime.Now.Year)
                {
                    //AtualizaCampeoesGastos(banco);
                    //AtualizaResumoMensal(banco);
                    AtualizaValorTotal(banco);
                }
            }
        }

        private void CarregaDadosCsv(AppDb banco, string file, int ano, Dictionary<string, UInt32> lstHash)
        {
            string sResumoValores = string.Empty;

            int indice = 0;
            int GABINETE = indice++;
            int NOME_DEPUTADO = indice++;
            int CPF_DEPUTADO = indice++;
            int NOME_FORNECEDOR = indice++;
            int CNPJ_CPF_FORNECEDOR = indice++;
            int DATA = indice++;
            int DOCUMENTO = indice++;
            int VALOR = indice++;
            int CPF_FORNECEDOR = 0;
            int CLASSIFICACAO = 0;

            if (ano == 2018 || ano == 2019)
            {
                indice = 0;
                NOME_DEPUTADO = indice++;
                CPF_DEPUTADO = indice++;
                NOME_FORNECEDOR = indice++;
                CNPJ_CPF_FORNECEDOR = indice++;
                CPF_FORNECEDOR = indice++;
                DOCUMENTO = indice++;
                DATA = indice++;
                VALOR = indice++;
                CLASSIFICACAO = indice++;
            }

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            {
                short count = 0;

                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                {
                    while (csv.Read())
                    {
                        count++;

                        if (count == 1)
                        {
                            if (ano == 2013 || ano == 2014)
                            {
                                if (
                                    (csv[GABINETE] != "Gabinete") ||
                                    (csv[NOME_DEPUTADO] != "Nome") ||
                                    (csv[CPF_DEPUTADO] != "CPF") ||
                                    (csv[NOME_FORNECEDOR] != "EMPRESA (OU PROFISSIONAL)") ||
                                    (csv[CNPJ_CPF_FORNECEDOR] != "CNPJ(ouCPF)") ||
                                    (csv[DATA] != "Data de Emissão") ||
                                    (csv[DOCUMENTO] != "NºDocumento") ||
                                    (csv[VALOR] != "Valor")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }
                            else if (ano == 2015)
                            {
                                if (
                                    (csv[GABINETE] != "GAB") ||
                                    (csv[NOME_DEPUTADO] != "DEPUTADO") ||
                                    (csv[CPF_DEPUTADO] != "CPF") ||
                                    (csv[NOME_FORNECEDOR] != "LOCAL") ||
                                    (csv[CNPJ_CPF_FORNECEDOR] != "CNPJ") ||
                                    (csv[DATA] != "DATA") ||
                                    (csv[DOCUMENTO] != "NUMERO") ||
                                    (csv[VALOR] != "VALOR")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }
                            else if (ano == 2016 || ano == 2017)
                            {
                                if (
                                    (csv[GABINETE] != "Gabinete") ||
                                    (csv[NOME_DEPUTADO] != "Nome") ||
                                    (csv[CPF_DEPUTADO] != "CPF") ||
                                    (csv[NOME_FORNECEDOR].ToUpper() != "EMPRESA (OU PROFISSIONAL)") ||
                                    (csv[CNPJ_CPF_FORNECEDOR] != "CNPJ (ou CPF)") ||
                                    (csv[DATA] != "Data de Emissão") ||
                                    (csv[DOCUMENTO] != "Nº Documento") ||
                                    (csv[VALOR].Trim() != "Valor")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }
                            else if (ano == 2018)
                            {
                                if (
                                    (csv[NOME_DEPUTADO] != "Nome do(a) Deputado(a)") ||
                                    (csv[CPF_DEPUTADO] != "CPF do(a) Deputado(a)") ||
                                    (csv[NOME_FORNECEDOR] != "Nome do Estabelecimento") ||
                                    (csv[CNPJ_CPF_FORNECEDOR] != "CNPJ") ||
                                    (csv[CPF_FORNECEDOR] != "CPF") ||
                                    (csv[DOCUMENTO] != "No.  do Recibo ou NF") ||
                                    (csv[DATA] != "Data do Recibo") ||
                                    (csv[VALOR] != "Valor") ||
                                    (csv[CLASSIFICACAO] != "Classificação")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }
                            else if (ano == 2019)
                            {
                                if (
                                    (csv[NOME_DEPUTADO] != "Nome do(a) Deputado(a)") ||
                                    (csv[CPF_DEPUTADO] != "CPF do(a) Deputado(a)") ||
                                    (csv[NOME_FORNECEDOR] != "Nome do Estabelecimento") ||
                                    (csv[CNPJ_CPF_FORNECEDOR] != "CNPJ") ||
                                    (csv[CPF_FORNECEDOR] != "CPF") ||
                                    (csv[DOCUMENTO] != "N°  do Recibo ou Nota Fiscal") ||
                                    (csv[DATA] != "Data do Recibo/NF") ||
                                    (csv[VALOR] != "Valor") ||
                                    (csv[CLASSIFICACAO] != "Classificação")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        if (string.IsNullOrEmpty(csv[NOME_DEPUTADO])) continue; //Linha vazia

                        banco.AddParameter("Nome", csv[NOME_DEPUTADO].Replace("Deputado", "").Replace("Deputada", ""));
                        banco.AddParameter("CPF", !string.IsNullOrEmpty(csv[CPF_DEPUTADO]) ? Utils.RemoveCaracteresNaoNumericos(csv[CPF_DEPUTADO]) : "");
                        banco.AddParameter("Empresa", csv[NOME_FORNECEDOR].Trim().Replace("NÃO INFORMADO", "").Replace("DOCUMENTO DANIFICADO", "").Replace("não consta documento", "").Trim());

                        string cnpj_cpf = "";
                        if (ano < 2018)
                        {
                            if (!string.IsNullOrEmpty(csv[CNPJ_CPF_FORNECEDOR]))
                            {
                                cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(csv[CNPJ_CPF_FORNECEDOR]);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(csv[CNPJ_CPF_FORNECEDOR]))
                            {
                                cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(csv[CNPJ_CPF_FORNECEDOR]);
                            }
                            else if (!string.IsNullOrEmpty(csv[CPF_FORNECEDOR]))
                            {
                                cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(csv[CPF_FORNECEDOR]);
                            }
                        }
                        banco.AddParameter("CNPJ_CPF", cnpj_cpf);

                        DateTime data;
                        if (DateTime.TryParse(csv[DATA], out data))
                        {
                            banco.AddParameter("DataEmissao", data);
                        }
                        else
                        {
                            // Quando a data não estiver difinida colocamos no feriado;
                            banco.AddParameter("DataEmissao", new DateTime(ano, 1, 1));
                        }

                        banco.AddParameter("Documento", csv[DOCUMENTO]);

                        string valor = csv[VALOR].Replace(" .", "").Replace(" ", "");

                        // Valor 1.500.00 é na verdade 1.500,00
                        Regex myRegex = new Regex(@"\.(\d\d$)", RegexOptions.Singleline);
                        if (myRegex.IsMatch(valor))
                        {
                            valor = myRegex.Replace(valor, @",$1");
                        }

                        try
                        {
                            banco.AddParameter("Valor", !string.IsNullOrEmpty(valor) ? (object)Convert.ToDouble(valor) : 0);
                        }
                        catch (Exception)
                        {
                            if (valor.EndsWith("."))
                            {
                                valor = valor.Substring(0, valor.Length - 1).Trim();
                            }

                            valor = valor.Replace(" ", "");

                            banco.AddParameter("Valor", !string.IsNullOrEmpty(valor) ? (object)Convert.ToDouble(valor) : 0);
                        }

                        if (ano < 2018)
                        {
                            banco.AddParameter("TipoDespesa", DBNull.Value);
                        }
                        else
                        {
                            banco.AddParameter("TipoDespesa", csv[CLASSIFICACAO]);
                        }

                        if (string.IsNullOrEmpty(cnpj_cpf))
                        {
                            banco.AddParameter("Observacao", csv[NOME_FORNECEDOR]);
                        }
                        else if (!Regex.IsMatch(cnpj_cpf, @"\d"))
                        {
                            banco.AddParameter("Observacao", cnpj_cpf + " - " + csv[NOME_FORNECEDOR]);
                        }
                        else
                        {
                            banco.AddParameter("Observacao", DBNull.Value);
                        }

                        banco.AddParameter("Ano", ano);

                        byte[] hash = banco.ParametersHash();
                        var key = Convert.ToHexString(hash);
                        if (lstHash.Remove(key))
                        {
                            banco.ClearParameters();
                            continue;
                        }

                        banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO tmp.cl_despesa_temp (
								nome, cpf, empresa, cnpj_cpf, data_emissao, documento, valor, tipo_despesa, observacao, ano, hash
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Documento, @Valor, @TipoDespesa, @Observacao, @Ano, @hash
							)");

                    }
                }
            }
        }

        private void CarregaDadosXlsx(AppDb banco, string file, int ano, Dictionary<string, UInt32> lstHash)
        {
            string sResumoValores = string.Empty;

            int indice = 1;
            int NOME_DEPUTADO = indice++;
            int CPF_DEPUTADO = indice++;
            int NOME_FORNECEDOR = indice++;
            int CNPJ_CPF_FORNECEDOR = indice++;
            int CPF_FORNECEDOR = indice++;
            int DOCUMENTO = indice++;
            int DATA = indice++;
            int VALOR = indice++;
            int CLASSIFICACAO = indice++;
            int OBSERVACAO = indice++;

            int count = 0;

            {
                using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
                using (var package = new ExcelPackage(reader.BaseStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    for (int i = 1; i <= worksheet.Dimension.End.Row; i++)
                    {
                        if (i == 1)
                        {

                            if (ano == 2019)
                            {
                                if (
                                    (worksheet.Cells[i, NOME_DEPUTADO].Value.ToString() != "Nome do(a) Deputado(a)") ||
                                    (worksheet.Cells[i, CPF_DEPUTADO].Value.ToString() != "CPF do(a) Deputado(a)") ||
                                    (worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString() != "Nome do Estabelecimento") ||
                                    (worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value.ToString() != "CNPJ") ||
                                    (worksheet.Cells[i, CPF_FORNECEDOR].Value.ToString() != "CPF") ||
                                    (worksheet.Cells[i, DOCUMENTO].Value.ToString() != "N°  do Recibo ou Nota Fiscal") ||
                                    (worksheet.Cells[i, DATA].Value.ToString() != "Data do Recibo/NF") ||
                                    (worksheet.Cells[i, VALOR].Value.ToString() != "Valor") ||
                                    (worksheet.Cells[i, CLASSIFICACAO].Value.ToString() != "Classificação")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }
                            else if (ano >= 2020)
                            {
                                // Ignorando a validação de 2 colunas pq esta com erro de envoding no arquivo original.
                                if (
                                    (worksheet.Cells[i, NOME_DEPUTADO].Value.ToString() != "Nome do Deputado") ||
                                    (worksheet.Cells[i, CPF_DEPUTADO].Value.ToString() != "CPF do Deputado") ||
                                    (worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString() != "Nome do Estabelecimento") ||
                                    (worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value.ToString() != "CNPJ") ||
                                    (worksheet.Cells[i, CPF_FORNECEDOR].Value.ToString() != "CPF") ||
                                    // (worksheet.Cells[i, DOCUMENTO].Value.ToString() != "Número do Recibo/NF") ||
                                    (worksheet.Cells[i, DATA].Value.ToString() != "Data do Recibo/NF") ||
                                    (worksheet.Cells[i, VALOR].Value.ToString() != "Valor (R$)")
                                // || (worksheet.Cells[i, CLASSIFICACAO].Value.ToString() != "Classificação")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        if (string.IsNullOrEmpty((string)worksheet.Cells[i, NOME_DEPUTADO].Value)) continue; //Linha vazia

                        banco.AddParameter("Nome", worksheet.Cells[i, NOME_DEPUTADO].Value.ToString().Replace("Deputado", "").Replace("Deputada", "").Trim());
                        banco.AddParameter("CPF", !string.IsNullOrEmpty(worksheet.Cells[i, CPF_DEPUTADO].Value.ToString()) ? Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[i, CPF_DEPUTADO].Value.ToString()) : "");
                        banco.AddParameter("Empresa", worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString().Trim().Replace("NÃO INFORMADO", "").Replace("DOCUMENTO DANIFICADO", "").Replace("não consta documento", "").Trim());

                        string cnpj_cpf = "";
                        if (!string.IsNullOrEmpty((string)worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value))
                        {
                            cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[i, CNPJ_CPF_FORNECEDOR].Value.ToString());
                        }
                        else if (!string.IsNullOrEmpty((string)worksheet.Cells[i, CPF_FORNECEDOR].Value))
                        {
                            cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(worksheet.Cells[i, CPF_FORNECEDOR].Value.ToString());
                        }

                        if (cnpj_cpf == "0030659700311234") cnpj_cpf = "00306597009834";
                        if (cnpj_cpf == "016152224000170") cnpj_cpf = "01615224000170";
                        banco.AddParameter("CNPJ_CPF", cnpj_cpf);

                        if (!string.IsNullOrEmpty(worksheet.Cells[i, DATA].Value?.ToString()))
                        {
                            if (worksheet.Cells[i, 7].Value is double)
                                banco.AddParameter("DataEmissao", DateTime.FromOADate((double)worksheet.Cells[i, DATA].Value));
                            else if (worksheet.Cells[i, 7].Value is DateTime)
                                banco.AddParameter("DataEmissao", (DateTime)worksheet.Cells[i, DATA].Value);
                            else
                            {
                                var data = worksheet.Cells[i, DATA].Value.ToString();
                                if (data.Split("/").Length != 3)
                                {
                                    data = data.Replace("/", "");
                                    data = $"{data.Substring(0, 2)}/{data.Substring(2, 2)}/{data.Substring(4, 4)}";
                                }

                                banco.AddParameter("DataEmissao", Convert.ToDateTime(data.Replace("31/05/22021", "31/05/2021")));
                            }

                        }
                        else
                        {
                            // Quando a data não estiver difinida colocamos no 1º do ano;
                            banco.AddParameter("DataEmissao", new DateTime(ano, 1, 1));
                        }

                        banco.AddParameter("Documento", worksheet.Cells[i, DOCUMENTO].Value);

                        string valor = worksheet.Cells[i, VALOR].Value.ToString();

                        // Valor 1.500.00 é na verdade 1.500,00
                        Regex myRegex = new Regex(@"\.(\d\d$)", RegexOptions.Singleline);
                        if (myRegex.IsMatch(valor))
                        {
                            valor = myRegex.Replace(valor, @",$1");
                        }

                        try
                        {
                            banco.AddParameter("Valor", !string.IsNullOrEmpty(valor) ? (object)Convert.ToDouble(valor) : 0);
                        }
                        catch (Exception)
                        {
                            if (valor.EndsWith("."))
                            {
                                valor = valor.Substring(0, valor.Length - 1).Trim();
                            }

                            valor = valor.Replace(" ", "").Replace("R$", "");

                            banco.AddParameter("Valor", !string.IsNullOrEmpty(valor) ? (object)Convert.ToDouble(valor) : 0);
                        }

                        banco.AddParameter("TipoDespesa", worksheet.Cells[i, CLASSIFICACAO].Value.ToString());

                        if (!string.IsNullOrEmpty(worksheet.Cells[i, OBSERVACAO].Value?.ToString()))
                        {
                            banco.AddParameter("Observacao", cnpj_cpf + " - " + worksheet.Cells[i, OBSERVACAO].Value.ToString());
                        }
                        else
                        {
                            banco.AddParameter("Observacao", DBNull.Value);
                        }

                        banco.AddParameter("Ano", ano);

                        byte[] hash = banco.ParametersHash();
                        var key = Convert.ToHexString(hash);
                        if (lstHash.Remove(key))
                        {
                            banco.ClearParameters();
                            continue;
                        }

                        banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO tmp.cl_despesa_temp (
								nome, cpf, empresa, cnpj_cpf, data_emissao, documento, valor, tipo_despesa, observacao, ano, hash
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Documento, @Valor, @TipoDespesa, @Observacao, @Ano, @hash
							)");

                        count++;
                    }
                }
            }
        }

        public void AtualizaValorTotal(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	    UPDATE cl_deputado dp SET
                    valor_total = IFNULL((
                        SELECT SUM(ds.valor) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
                    ), 0);");
        }

        private void InsereTipoDespesaFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_despesa_tipo (descricao)
        	        select distinct tipo_despesa
        	        from tmp.cl_despesa_temp
        	        where tipo_despesa is not null
                    and tipo_despesa not in (
        		        select descricao from cl_despesa_tipo
        	        );
                ");

            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} tipos de despesas incluidos!", banco.RowsAffected);
            }
        }

        private void InsereDeputadoFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_deputado (nome_parlamentar, cpf, id_estado)
        	        select distinct Nome, cpf, 53
        	        from tmp.cl_despesa_temp
        	        where cpf not in (
        		        select cpf from cl_deputado where id_estado = 53
        	        );
                ");

            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} parlamentares incluidos!", banco.RowsAffected);
            }
        }

        private void InsereFornecedorFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				    INSERT INTO fornecedor (nome, cnpj_cpf)
				    select MAX(dt.empresa), dt.cnpj_cpf
				    from tmp.cl_despesa_temp dt
				    left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				    where dt.cnpj_cpf is not null
				    and f.id is null
                    -- and LENGTH(dt.cnpj_cpf) <= 14
				    GROUP BY dt.cnpj_cpf;
			    ");

            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} fornecedores incluidos!", banco.RowsAffected);
            }

        }

        private void InsereDespesaFinal(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
                UPDATE tmp.cl_despesa_temp SET cnpj_cpf = NULL WHERE cnpj_cpf = '';
                UPDATE tmp.cl_despesa_temp SET observacao = NULL WHERE observacao = 'não consta documento';

				INSERT INTO cl_despesa (
					id_cl_deputado,
                    id_cl_despesa_tipo,
	                id_fornecedor,
	                data,
	                ano_mes,
	                numero_documento,
	                valor,
                    observacao,
                    hash
				)
                SELECT 
	                p.id AS id_cl_deputado,
                    dt.id AS id_cl_despesa_tipo,
                    IFNULL(f.id, 82624) AS id_fornecedor,
                    d.data_emissao,
                    concat(year(d.data_emissao), LPAD(month(d.data_emissao), 2, '0')) AS ano_mes,
                    d.documento AS numero_documento,
                    d.valor AS valor,
                    d.observacao AS observacao,
                    d.hash
                FROM tmp.cl_despesa_temp d
                inner join cl_deputado p on p.cpf = d.cpf
                left join cl_despesa_tipo dt on dt.descricao = d.tipo_despesa
                LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
                ORDER BY d.id;
			", 3600);


            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} despesas incluidas!", banco.RowsAffected);
            }

        }

        private void LimpaDespesaTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table tmp.cl_despesa_temp;
			");
        }

    }
}
