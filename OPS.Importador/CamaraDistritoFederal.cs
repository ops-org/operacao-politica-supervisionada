using CsvHelper;
using OfficeOpenXml;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OPS.Importador
{
    public static class CamaraDistritoFederal
    {
        public static string ImportarDespesas(string atualDir, int ano, bool completo)
        {
            string downloadUrl;
            if (ano <= 2017)
            {
                downloadUrl = string.Format("http://dadosabertos.cl.df.gov.br/View/opendata/verbas/verba_indenizatoria_{0}.csv", ano);
            }
            else if (ano == 2018)
            {
                downloadUrl = string.Format("http://dadosabertos.cl.df.gov.br/View/opendata/verbas/{0}%20completa%20verba_indenizatoria.csv", ano);
            }
            else
            {
                throw new NotImplementedException();
            }

            var fullFileNameCsv = atualDir + @"\CLDF_" + ano + ".csv";

            if (!Directory.Exists(atualDir))
                Directory.CreateDirectory(atualDir);

            var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

            request.UserAgent = "Other";
            request.Method = "HEAD";
            request.ContentType = "application/json;charset=UTF-8";
            request.Timeout = 1000000;

            using (var resp = request.GetResponse())
            {
                if (File.Exists(fullFileNameCsv))
                {
                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                    long ContentLengthLocal = 0;

                    if (File.Exists(fullFileNameCsv))
                        ContentLengthLocal = new FileInfo(fullFileNameCsv).Length;

                    if (!completo && ContentLength == ContentLengthLocal)
                    {
                        Console.WriteLine("Não há novos itens para importar!");
                        return "<p>Não há novos itens para importar!</p>";
                    }
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");
                    client.DownloadFile(downloadUrl, fullFileNameCsv);
                }
            }

            try
            {
                var resumoImportacao = CarregaDadosCsv(fullFileNameCsv, ano);

                //using (var banco = new Banco())
                //{
                //    banco.ExecuteNonQuery(@"
                //	UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
                //");
                //}

                return resumoImportacao;
            }
            catch (Exception ex)
            {
                //Excluir o arquivo para tentar importar novamente na proxima execução
                //File.Delete(fullFileNameCsv);

                return "Erro ao importar:" + ex.Message;
            }
        }

        public static string ImportarDespesasNovo(string atualDir, int ano, bool completo)
        {
            string resumoImportacao = "";
            CultureInfo usEnglish = new CultureInfo("pt-BR");
            int ultimoMes = 12;
            if (DateTime.Today.Year == ano)
            {
                ultimoMes = DateTime.Today.Month;
            }

            for (int mes = 1; mes <= ultimoMes; mes++)
            {
                DateTimeFormatInfo portugueseInfo = usEnglish.DateTimeFormat;
                string nomeMes = portugueseInfo.MonthNames[mes - 1];

                if (ano == 2019 && mes <= 2)
                {
                    nomeMes = nomeMes.Substring(0, 3);
                }

                string downloadUrl = string.Format("http://dadosabertos.cl.df.gov.br/View/opendata/verbas/{0}_{1}_verba%20indenizatoria.xlsx", ano, nomeMes.ToUpper());
                var fullFileNameCsv = atualDir + @"\CLDF_" + ano + "_" + mes + ".xlsx";

                if (!Directory.Exists(atualDir))
                    Directory.CreateDirectory(atualDir);

                var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

                request.UserAgent = "Other";
                request.Method = "HEAD";
                request.ContentType = "application/json;charset=UTF-8";
                request.Timeout = 1000000;

                try
                {
                    using (var resp = request.GetResponse())
                    {
                        if (File.Exists(fullFileNameCsv))
                        {
                            var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                            long ContentLengthLocal = 0;

                            if (File.Exists(fullFileNameCsv))
                                ContentLengthLocal = new FileInfo(fullFileNameCsv).Length;

                            if (!completo && ContentLength == ContentLengthLocal)
                            {
                                Console.WriteLine("Não há novos itens para importar!");
                                return "<p>Não há novos itens para importar!</p>";
                            }
                        }

                        using (var client = new WebClient())
                        {
                            client.Headers.Add("User-Agent: Other");
                            client.DownloadFile(downloadUrl, fullFileNameCsv);
                        }
                    }

                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("(404)"))
                        throw;
                    else
                        return string.Empty;
                }

                try
                {
                    resumoImportacao += CarregaDadosXlsx(fullFileNameCsv, ano, mes);

                    //using (var banco = new Banco())
                    //{
                    //    banco.ExecuteNonQuery(@"
                    // UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
                    //");
                    //}


                }
                catch (Exception ex)
                {
                    //Excluir o arquivo para tentar importar novamente na proxima execução
                    //File.Delete(fullFileNameCsv);

                    return "Erro ao importar:" + ex.Message;
                }
            }

            return resumoImportacao;
        }

        private static string CarregaDadosCsv(string file, int ano)
        {
            var sb = new StringBuilder();
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


            int linhaAtual = 0;

            using (var banco = new AppDb())
            {
                //var lstHash = new Dictionary<string, long>();
                //using (var dReader = banco.ExecuteReader("select id, hash from cl_despesa where ano=" + ano))
                //{
                //    while (dReader.Read())
                //    {
                //        lstHash.Add(dReader["hash"].ToString(), Convert.ToInt64(dReader["id"]));
                //    }
                //}

                //using (var dReader = banco.ExecuteReader(string.Format("select sum(valor) as valor, count(1) as itens from cl_despesa where ano_mes between {0}01 and {0}12", ano)))
                //{
                //    if (dReader.Read())
                //    {
                //        sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                //    }
                //}

                LimpaDespesaTemporaria(banco);

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short count = 0;

                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                    {
                        while (csv.Read())
                        {
                            //Id = csv.GetField<int>("Id")
                            //Name = csv.GetField("Name")

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


                            //string hash = banco.ParametersHash();
                            //if (lstHash.Remove(hash))
                            //{
                            //    banco.ClearParameters();
                            //    continue;
                            //}

                            //banco.AddParameter("hash", hash);

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

                            banco.ExecuteNonQuery(
                                @"INSERT INTO cl_despesa_temp (
								nome, cpf, empresa, cnpj_cpf, data_emissao, documento, valor, tipo_despesa, observacao, ano
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Documento, @Valor, @TipoDespesa, @Observacao, @Ano
							)");

                        }
                    }

                    if (++linhaAtual % 100 == 0)
                    {
                        Console.WriteLine(linhaAtual);
                    }
                }

                //if (lstHash.Count == 0 && linhaAtual == 0)
                //{
                //    sb.AppendFormat("<p>Não há novos itens para importar! #2</p>");
                //    return sb.ToString();
                //}

                //if (lstHash.Count > 0)
                //{
                //    string lstExcluir = lstHash.Aggregate("", (keyString, pair) => keyString + "," + pair.Value);
                //    banco.ExecuteNonQuery(string.Format("delete from sf_despesa where id IN({0})", lstExcluir.Substring(1)));

                //    Console.WriteLine("Registros para exluir: " + lstHash.Count);
                //    sb.AppendFormat("<p>{0} registros excluidos</p>", lstHash.Count);
                //}

                sb.Append(ProcessarDespesasTemp(banco));
            }

            //if (ano == DateTime.Now.Year)
            //{
            //    //AtualizaCampeoesGastos();
            //    //AtualizaResumoMensal();
            //    AtualizaValorTotal();
            //}

            using (var banco = new AppDb())
            {
                using (var dReader = banco.ExecuteReader(string.Format("select sum(valor) as valor, count(1) as itens from cl_despesa where ano_mes between {0}01 and {0}12", ano)))
                {
                    if (dReader.Read())
                    {
                        sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                    }
                }

                sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
            }

            return sb.ToString();
        }

        private static string CarregaDadosXlsx(string file, int ano, int mes)
        {
            var sb = new StringBuilder();
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

            int linhaAtual = 0;
            int inserido = 0;

            // Controle, estão vindo itens duplicados no XML
            var lstHash = new Dictionary<string, long>();

            // Controle, lista para remover caso não constem no XML
            var lstHashExcluir = new Dictionary<string, long>();

            using (var banco = new AppDb())
            {
                using (var dReader = banco.ExecuteReader(string.Format("select id, hash from cl_despesa where ano_mes = {0}{1:00}", ano, mes)))
                {
                    while (dReader.Read())
                    {
                        if (!lstHash.ContainsKey(dReader["hash"].ToString()))
                        {
                            lstHash.Add(dReader["hash"].ToString(), Convert.ToInt64(dReader["id"]));
                            lstHashExcluir.Add(dReader["hash"].ToString(), Convert.ToInt64(dReader["id"]));
                        }
                    }
                }

                using (var dReader = banco.ExecuteReader(string.Format("select sum(valor) as valor, count(1) as itens from cl_despesa where ano_mes = {0}{1:00}", ano, mes)))
                {
                    if (dReader.Read())
                    {
                        sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                    }
                }

                LimpaDespesaTemporaria(banco);

                using (var package = new ExcelPackage(new FileInfo(file)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    for (int i = 1; i <= worksheet.Dimension.End.Row; i++)
                    {
                        if (i == 1)
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

                            // Pular linha de titulo
                            continue;
                        }

                        if (string.IsNullOrEmpty((string)worksheet.Cells[i, NOME_DEPUTADO].Value)) continue; //Linha vazia

                        banco.AddParameter("Nome", worksheet.Cells[i, NOME_DEPUTADO].Value.ToString().Replace("Deputado", "").Replace("Deputada", ""));
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
                        banco.AddParameter("CNPJ_CPF", cnpj_cpf);

                        if (worksheet.Cells[i, DATA].Value != null)
                        {
                            banco.AddParameter("DataEmissao", (DateTime)worksheet.Cells[i, DATA].Value);
                        }
                        else
                        {
                            // Quando a data não estiver difinida colocamos no feriado;
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

                            valor = valor.Replace(" ", "");

                            banco.AddParameter("Valor", !string.IsNullOrEmpty(valor) ? (object)Convert.ToDouble(valor) : 0);
                        }

                        string hash = banco.ParametersHash();
                        if (lstHash.ContainsKey(hash))
                        {
                            lstHashExcluir.Remove(hash);
                            banco.ClearParameters();
                            continue;
                        }

                        banco.AddParameter("hash", hash);
                        banco.AddParameter("TipoDespesa", worksheet.Cells[i, CLASSIFICACAO].Value.ToString());

                        if (string.IsNullOrEmpty(cnpj_cpf))
                        {
                            banco.AddParameter("Observacao", worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString());
                        }
                        else if (!Regex.IsMatch(cnpj_cpf, @"\d"))
                        {
                            banco.AddParameter("Observacao", cnpj_cpf + " - " + worksheet.Cells[i, NOME_FORNECEDOR].Value.ToString());
                        }
                        else
                        {
                            banco.AddParameter("Observacao", DBNull.Value);
                        }

                        banco.AddParameter("Ano", ano);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO cl_despesa_temp (
								nome, cpf, empresa, cnpj_cpf, data_emissao, documento, valor, tipo_despesa, observacao, ano, hash
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Documento, @Valor, @TipoDespesa, @Observacao, @Ano, @hash
							)");

                        inserido++;
                    }
                }

                if (++linhaAtual % 100 == 0)
                {
                    Console.WriteLine(linhaAtual);
                }

                sb.Append(ProcessarDespesasTemp(banco, string.Format("{0}{1:00}", ano, mes)));
            }


            //if (ano == DateTime.Now.Year)
            //{
            //    //AtualizaCampeoesGastos();
            //    //AtualizaResumoMensal();
            //    AtualizaValorTotal();
            //}

            using (var banco = new AppDb())
            {
                if (lstHashExcluir.Count > 0)
                {
                    int skip = 0;
                    while (true)
                    {
                        var lstHashExcluirTemp = lstHashExcluir.Skip(skip).Take(50);
                        if (lstHashExcluirTemp.Count() == 0) break;

                        string lstExcluir = lstHashExcluirTemp.Aggregate("", (keyString, pair) => keyString + "," + pair.Value);
                        banco.ExecuteNonQuery(string.Format("delete from cf_despesa where id IN({0})", lstExcluir.Substring(1)));

                        skip += 50;
                    }

                    Console.WriteLine("Registros para exluir: " + lstHashExcluir.Count);
                    sb.AppendFormat("<p>{0} registros excluidos</p>", lstHashExcluir.Count);
                }
                else if (inserido == 0)
                {
                    sb.Append("<p>Não há novos itens para importar! #2</p>");
                    return sb.ToString();
                }

                using (var dReader = banco.ExecuteReader(string.Format("select sum(valor) as valor, count(1) as itens from cl_despesa where ano_mes = {0}{1:00}", ano, mes)))
                {
                    if (dReader.Read())
                    {
                        sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                    }
                }

                sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
            }

            return sb.ToString();
        }

        public static void AtualizaValorTotal()
        {
            using (var banco = new AppDb())
            {
                banco.ExecuteNonQuery(@"
        	        UPDATE cl_deputado dp SET
                        valor_total = (
                            SELECT SUM(ds.valor) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
                        );");
            }
        }

        private static string ProcessarDespesasTemp(AppDb banco, string referencia = "")
        {
            var sb = new StringBuilder();

            sb.Append(InsereTipoDespesaFaltante(banco));
            sb.Append(InsereDeputadoFaltante(banco));
            sb.Append(InsereFornecedorFaltante(banco));
            sb.Append(InsereDespesaFinal(banco, referencia));
            LimpaDespesaTemporaria(banco);

            return sb.ToString();
        }

        private static string InsereTipoDespesaFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_despesa_tipo (descricao)
        	        select distinct tipo_despesa
        	        from cl_despesa_temp
        	        where tipo_despesa is not null
                    and tipo_despesa not in (
        		        select descricao from cl_despesa_tipo
        	        );
                ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Deputado</p>";
            }

            return string.Empty;
        }

        private static string InsereDeputadoFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_deputado (nome_parlamentar, cpf, id_estado)
        	        select distinct Nome, cpf, 53
        	        from cl_despesa_temp
        	        where cpf not in (
        		        select cpf from cl_deputado
        	        );
                ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Deputado</p>";
            }

            return string.Empty;
        }

        private static string InsereFornecedorFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				    INSERT INTO fornecedor (nome, cnpj_cpf)
				    select MAX(dt.empresa), dt.cnpj_cpf
				    from cl_despesa_temp dt
				    left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				    where dt.cnpj_cpf is not null
				    and f.id is null
                    -- and LENGTH(dt.cnpj_cpf) <= 14
				    GROUP BY dt.cnpj_cpf;
			    ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Fornecedor</p>";
            }

            return string.Empty;
        }

        private static string InsereDespesaFinal(AppDb banco, string referencia)
        {
            if (string.IsNullOrEmpty(referencia))
            {
                referencia = "concat(year(d.data_emissao), LPAD(month(d.data_emissao), 2, '0')) AS ano_mes";
            }

            banco.ExecuteNonQuery(string.Format(@"
                UPDATE cl_despesa_temp SET cnpj_cpf = NULL WHERE cnpj_cpf = '';
                UPDATE cl_despesa_temp SET observacao = NULL WHERE observacao = 'não consta documento';

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
                    {0},
                    d.documento AS numero_documento,
                    d.valor AS valor,
                    d.observacao AS observacao,
                    d.hash
                FROM cl_despesa_temp d
                inner join cl_deputado p on p.cpf = d.cpf
                left join cl_despesa_tipo dt on dt.descricao = d.tipo_despesa
                LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
                ORDER BY d.id;
			", referencia), 3600);

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Despesa</p>";
            }

            return string.Empty;
        }

        private static void LimpaDespesaTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table cl_despesa_temp;
			");
        }
    }
}
