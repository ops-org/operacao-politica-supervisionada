using CsvHelper;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OPS.ImportacaoDados
{
    public static class CamaraDistritoFederal
    {
        public static string ImportarDespesas(string atualDir, int ano, bool completo)
        {
            string downloadUrl;
            if (ano < 2018)
            {
                downloadUrl = string.Format("http://dadosabertos.cl.df.gov.br/View/opendata/verbas/verba_indenizatoria_{0}.csv", ano);
            }
            else
            {
                downloadUrl = string.Format("http://dadosabertos.cl.df.gov.br/View/opendata/verbas/{0}%20completa%20verba_indenizatoria.csv", ano);
            }

            var fullFileNameCsv = atualDir + @"\CLDF_" + ano + ".csv";

            //if (!Directory.Exists(atualDir))
            //    Directory.CreateDirectory(atualDir);

            //var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

            //request.UserAgent = "Other";
            //request.Method = "HEAD";
            //request.ContentType = "application/json;charset=UTF-8";
            //request.Timeout = 1000000;

            //using (var resp = request.GetResponse())
            //{
            //    if (File.Exists(fullFileNameCsv))
            //    {
            //        var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
            //        long ContentLengthLocal = 0;

            //        if (File.Exists(fullFileNameCsv))
            //            ContentLengthLocal = new FileInfo(fullFileNameCsv).Length;

            //        if (!completo && ContentLength == ContentLengthLocal)
            //        {
            //            Console.WriteLine("Não há novos itens para importar!");
            //            return "<p>Não há novos itens para importar!</p>";
            //        }
            //    }

            //    using (var client = new WebClient())
            //    {
            //        client.Headers.Add("User-Agent: Other");
            //        client.DownloadFile(downloadUrl, fullFileNameCsv);
            //    }
            //}

            try
            {
                var resumoImportacao = CarregaDadosCsv(fullFileNameCsv, ano);

                //            using (var banco = new Banco())
                //            {
                //                banco.ExecuteNonQuery(@"
                //	UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
                //");
                //            }

                return resumoImportacao;
            }
            catch (Exception ex)
            {
                // Excluir o arquivo para tentar importar novamente na proxima execução
                //File.Delete(fullFileNameCsv);

                return "Erro ao importar:" + ex.Message;
            }
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

            using (var banco = new Banco())
            {
                //var lstHash = new Dictionary<string, long>();
                //using (var dReader = banco.ExecuteReader("select id, hash from sf_despesa where ano=" + ano))
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

                //LimpaDespesaTemporaria(banco);

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short count = 0;

                    using (var csv = new CsvReader(reader))
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
                                else if (!string.IsNullOrEmpty(csv[CNPJ_CPF_FORNECEDOR]))
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

                            string valor = csv[VALOR];

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
                            catch (Exception e)
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
								Nome, CPF, Empresa, CNPJ_CPF, DataEmissao, Documento, Valor, TipoDespesa, Observacao, Ano
							) VALUES (
								@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Documento, @Valor, @TipoDespesa, @Observacao, @Ano
							)");

                        }
                    }


                    //             while (!reader.EndOfStream)
                    //             {
                    //                 count++;

                    //                 var linha = reader.ReadLine();
                    //                 if (string.IsNullOrEmpty(linha))
                    //                     continue;

                    //                 if (ano != 2016 && ano != 2018 && ano != 2019)
                    //                 {
                    //                     while (!linha.EndsWith(";"))
                    //                     {
                    //                         linha += reader.ReadLine();
                    //                     }
                    //                 }
                    //                 else
                    //                 {
                    //                     if (linha.Split(new[] { ";" }, StringSplitOptions.None).Length < 8)
                    //                     {
                    //                         linha += reader.ReadLine();
                    //                     }
                    //                 }

                    //                 if (string.IsNullOrEmpty(linha) || linha == ";;;;;;;;") continue;

                    //                 linha = linha.Replace("B ; M 10 PRODUÇÕES LTDA ME", "B & M 10 PRODUÇÕES LTDA ME");

                    //                 var valores = linha.Split(new[] { ";" }, StringSplitOptions.None).ToList();

                    //                 if (count == 1)
                    //                 {
                    //                     if (ano == 2013 || ano == 2014)
                    //                     {
                    //                         if (
                    //                             (valores[GABINETE] != "Gabinete") ||
                    //                             (valores[NOME_DEPUTADO] != "Nome") ||
                    //                             (valores[CPF_DEPUTADO] != "CPF") ||
                    //                             (valores[NOME_FORNECEDOR] != "EMPRESA (OU PROFISSIONAL)") ||
                    //                             (valores[CNPJ_CPF_FORNECEDOR] != "CNPJ(ouCPF)") ||
                    //                             (valores[DATA] != "Data de Emissão") ||
                    //                             (valores[DOCUMENTO] != "NºDocumento") ||
                    //                             (valores[VALOR] != "Valor")
                    //                         )
                    //                         {
                    //                             throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                    //                         }
                    //                     }
                    //                     else if (ano == 2015)
                    //                     {
                    //                         if (
                    //                             (valores[GABINETE] != "GAB") ||
                    //                             (valores[NOME_DEPUTADO] != "DEPUTADO") ||
                    //                             (valores[CPF_DEPUTADO] != "CPF") ||
                    //                             (valores[NOME_FORNECEDOR] != "LOCAL") ||
                    //                             (valores[CNPJ_CPF_FORNECEDOR] != "CNPJ") ||
                    //                             (valores[DATA] != "DATA") ||
                    //                             (valores[DOCUMENTO] != "NUMERO") ||
                    //                             (valores[VALOR] != "VALOR")
                    //                         )
                    //                         {
                    //                             throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                    //                         }
                    //                     }
                    //                     else if (ano == 2016 || ano == 2017)
                    //                     {
                    //                         if (
                    //                             (valores[GABINETE] != "Gabinete") ||
                    //                             (valores[NOME_DEPUTADO] != "Nome") ||
                    //                             (valores[CPF_DEPUTADO] != "CPF") ||
                    //                             (valores[NOME_FORNECEDOR].ToUpper() != "EMPRESA (OU PROFISSIONAL)") ||
                    //                             (valores[CNPJ_CPF_FORNECEDOR] != "CNPJ (ou CPF)") ||
                    //                             (valores[DATA] != "Data de Emissão") ||
                    //                             (valores[DOCUMENTO] != "Nº Documento") ||
                    //                             (valores[VALOR].Trim() != "Valor")
                    //                         )
                    //                         {
                    //                             throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                    //                         }
                    //                     }
                    //                     else if (ano == 2018)
                    //                     {
                    //                         if (
                    //                             (valores[NOME_DEPUTADO] != "Nome do(a) Deputado(a)") ||
                    //                             (valores[CPF_DEPUTADO] != "CPF do(a) Deputado(a)") ||
                    //                             (valores[NOME_FORNECEDOR] != "Nome do Estabelecimento") ||
                    //                             (valores[CNPJ_CPF_FORNECEDOR] != "CNPJ") ||
                    //                             (valores[CPF_FORNECEDOR] != "CPF") ||
                    //                             (valores[DOCUMENTO] != "No.  do Recibo ou NF") ||
                    //                             (valores[DATA] != "Data do Recibo") ||
                    //                             (valores[VALOR] != "Valor") ||
                    //                             (valores[CLASSIFICACAO] != "Classificação")
                    //                         )
                    //                         {
                    //                             throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                    //                         }
                    //                     }
                    //                     else if (ano == 2019)
                    //                     {
                    //                         if (
                    //                             (valores[NOME_DEPUTADO] != "Nome do(a) Deputado(a)") ||
                    //                             (valores[CPF_DEPUTADO] != "CPF do(a) Deputado(a)") ||
                    //                             (valores[NOME_FORNECEDOR] != "Nome do Estabelecimento") ||
                    //                             (valores[CNPJ_CPF_FORNECEDOR] != "CNPJ") ||
                    //                             (valores[CPF_FORNECEDOR] != "CPF") ||
                    //                             (valores[DOCUMENTO] != "N°  do Recibo ou Nota Fiscal") ||
                    //                             (valores[DATA] != "Data do Recibo/NF") ||
                    //                             (valores[VALOR] != "Valor") ||
                    //                             (valores[CLASSIFICACAO] != "Classificação")
                    //                         )
                    //                         {
                    //                             throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                    //                         }
                    //                     }

                    //                     // Pular linha de titulo
                    //                     continue;
                    //                 }

                    //                 for (int i = 0; i < valores.Count; i++)
                    //                 {
                    //                     valores[i] = valores[i].Trim();

                    //                     if (valores[i].StartsWith("\""))
                    //                     {
                    //                         valores[i] = valores[i].Substring(1, valores[i].Length - 2).Trim();
                    //                     }
                    //                 }

                    //                 banco.AddParameter("Nome", valores[NOME_DEPUTADO].Replace("Deputado", "").Replace("Deputada", ""));
                    //                 banco.AddParameter("CPF", !string.IsNullOrEmpty(valores[CPF_DEPUTADO]) ? Utils.RemoveCaracteresNaoNumericos(valores[CPF_DEPUTADO]) : "");
                    //                 banco.AddParameter("Empresa", valores[NOME_FORNECEDOR].Replace("DOCUMENTO DANIFICADO", "").Trim());

                    //                 string cnpj_cpf = "";
                    //                 if (ano < 2018)
                    //                 {
                    //                     if (!string.IsNullOrEmpty(valores[CNPJ_CPF_FORNECEDOR]))
                    //                     {
                    //                         cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF_FORNECEDOR]);
                    //                     }
                    //                 }
                    //                 else
                    //                 {
                    //                     if (!string.IsNullOrEmpty(valores[CNPJ_CPF_FORNECEDOR]))
                    //                     {
                    //                         cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF_FORNECEDOR]);
                    //                     }
                    //                     else if (!string.IsNullOrEmpty(valores[CNPJ_CPF_FORNECEDOR]))
                    //                     {
                    //                         cnpj_cpf = Utils.RemoveCaracteresNaoNumericos(valores[CPF_FORNECEDOR]);
                    //                     }
                    //                 }
                    //                 banco.AddParameter("CNPJ_CPF", cnpj_cpf);

                    //                 DateTime data;
                    //                 if (DateTime.TryParse(valores[DATA], out data))
                    //                 {
                    //                     banco.AddParameter("DataEmissao", data);
                    //                 }
                    //                 else
                    //                 {
                    //                     // Quando a data não estiver difinida colocamos no feriado;
                    //                     banco.AddParameter("DataEmissao", new DateTime(ano, 1, 1));
                    //                 }

                    //                 banco.AddParameter("Documento", valores[DOCUMENTO]);

                    //                 if (valores[VALOR].EndsWith(".00"))
                    //                 {
                    //                     // Valor 1.500.00 é na verdade 1.500,00
                    //                     valores[VALOR] = valores[VALOR].Substring(0, valores[VALOR].Length - 3).Trim();
                    //                 }

                    //                 try
                    //                 {
                    //                     banco.AddParameter("Valor", !string.IsNullOrEmpty(valores[VALOR]) ? (object)Convert.ToDouble(valores[VALOR]) : 0);
                    //                 }
                    //                 catch (Exception e)
                    //                 {
                    //                     if (valores[VALOR].EndsWith("."))
                    //                     {
                    //                         valores[VALOR] = valores[VALOR].Substring(0, valores[VALOR].Length - 1).Trim();
                    //                     }

                    //                     valores[VALOR] = valores[VALOR].Replace(" ", "");

                    //                     banco.AddParameter("Valor", !string.IsNullOrEmpty(valores[VALOR]) ? (object)Convert.ToDouble(valores[VALOR]) : 0);
                    //                 }


                    //                 //string hash = banco.ParametersHash();
                    //                 //if (lstHash.Remove(hash))
                    //                 //{
                    //                 //    banco.ClearParameters();
                    //                 //    continue;
                    //                 //}

                    //                 //banco.AddParameter("hash", hash);

                    //                 if (ano < 2018)
                    //                 {
                    //                     banco.AddParameter("TipoDespesa", DBNull.Value);
                    //                 }
                    //                 else
                    //                 {
                    //                     banco.AddParameter("TipoDespesa", valores[CLASSIFICACAO]);
                    //                 }

                    //                 if (string.IsNullOrEmpty(cnpj_cpf))
                    //                 {
                    //                     banco.AddParameter("Observacao", valores[NOME_FORNECEDOR]);
                    //                 }
                    //                 else if (!Regex.IsMatch(cnpj_cpf, @"\d"))
                    //                 {
                    //                     banco.AddParameter("Observacao", cnpj_cpf + " - " + valores[NOME_FORNECEDOR]);
                    //                 }
                    //                 else
                    //                 {
                    //                     banco.AddParameter("Observacao", DBNull.Value);
                    //                 }

                    //                 banco.ExecuteNonQuery(
                    //                     @"INSERT INTO cl_despesa_temp (
                    //	Nome, CPF, Empresa, CNPJ_CPF, DataEmissao, Documento, Valor, TipoDespesa, Observacao
                    //) VALUES (
                    //	@Nome, @CPF, @Empresa, @CNPJ_CPF, @DataEmissao, @Documento, @Valor, @TipoDespesa, @Observacao
                    //)");

                    //             }

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

                //sb.Append(ProcessarDespesasTemp(banco));
            }



            //if (ano == DateTime.Now.Year)
            //{
            //    //AtualizaCampeoesGastos();
            //    //AtualizaResumoMensal();
            //    AtualizaValorTotal();
            //}

            //using (var banco = new Banco())
            //{
            //    using (var dReader = banco.ExecuteReader(string.Format("select sum(valor) as valor, count(1) as itens from cl_despesa where ano_mes between {0}01 and {0}12", ano)))
            //    {
            //        if (dReader.Read())
            //        {
            //            sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
            //        }
            //    }

            //    sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
            //}

            return sb.ToString();
        }

        private static void AtualizaValorTotal()
        {
            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(@"
        	        UPDATE cl_deputado dp SET
                        valor_total = (
                            SELECT SUM(ds.valor) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
                        );");
                //  WHERE dp.id IN(
                //    SELECT DISTINCT id_cl_deputado from cl_despesa WHERE YEAR(DATA) =
                //  );
            }
        }

        private static string ProcessarDespesasTemp(Banco banco)
        {
            var sb = new StringBuilder();

            sb.Append(InsereTipoDespesaFaltante(banco));
            sb.Append(InsereDeputadoFaltante(banco));
            sb.Append(InsereFornecedorFaltante(banco));
            sb.Append(InsereDespesaFinal(banco));
            LimpaDespesaTemporaria(banco);

            return sb.ToString();
        }

        private static string InsereTipoDespesaFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_despesa_tipo (descricao)
        	        select distinct TipoDespesa
        	        from cl_despesa_temp
        	        where TipoDespesa is not null
                    and TipoDespesa not in (
        		        select descricao from cl_despesa_tipo
        	        );
                ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Deputado</p>";
            }

            return string.Empty;
        }

        private static string InsereDeputadoFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_deputado (nome_parlamentar, cpf)
        	        select distinct Nome, CPF
        	        from cl_despesa_temp
        	        where CPF not in (
        		        select cpf from cl_deputado
        	        );
                ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Deputado</p>";
            }

            return string.Empty;
        }

        private static string InsereFornecedorFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				    INSERT INTO fornecedor (nome, cnpj_cpf)
				    select MAX(dt.Empresa), dt.CNPJ_CPF
				    from cl_despesa_temp dt
				    left join fornecedor f on f.cnpj_cpf = dt.CNPJ_CPF
				    where dt.CNPJ_CPF is not null
				    and f.id is null
                    -- and LENGTH(dt.CNPJ_CPF) <= 14
				    GROUP BY dt.CNPJ_CPF;
			    ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Fornecedor</p>";
            }

            return string.Empty;
        }

        private static string InsereDespesaFinal(Banco banco)
        {
            banco.ExecuteNonQuery(@"
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
                    observacao
				)
                SELECT 
                    id_cl_deputado,
                    id_cl_despesa_tipo,
	                id_fornecedor,
	                data,
	                ano_mes,
	                numero_documento,
	                valor,
                    observacao
                FROM (
                    SELECT 
	                    d.id,
	                    p.id AS id_cl_deputado,
                            dt.id AS id_cl_despesa_tipo,
                           f.id AS id_fornecedor,
                           d.DataEmissao AS data,
                           concat(year(d.DataEmissao), LPAD(month(d.DataEmissao), 2, '0')) AS ano_mes,
                           d.Documento AS numero_documento,
                           d.Valor AS valor,
                            d.Observacao AS observacao
                    FROM cl_despesa_temp d
                    inner join cl_deputado p on p.cpf = d.CPF
                    left join cl_despesa_tipo dt on dt.descricao = d.TipoDespesa
                    LEFT join fornecedor f on f.cnpj_cpf = d.CNPJ_CPF
                    WHERE d.CNPJ_CPF IS NOT NULL 

                    UNION ALL

                    SELECT 
	                    d.id,
	                    p.id,
                            dt.id,
                           IFNULL(f.id, 82624),
                           d.DataEmissao,
                           concat(year(d.DataEmissao), LPAD(month(d.DataEmissao), 2, '0')),
                           d.Documento,
                           d.Valor,
                            d.Observacao
                    FROM cl_despesa_temp d
                    inner join cl_deputado p on p.cpf = d.CPF
                    left join cl_despesa_tipo dt on dt.descricao = d.TipoDespesa
                    LEFT JOIN (
	                    SELECT id, nome from fornecedor GROUP BY nome
                    ) f ON d.Empresa = f.nome
                    WHERE d.CNPJ_CPF IS NULL
                ) x

                ORDER BY valor desc, id;
			", 3600);

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Despesa</p>";
            }

            return string.Empty;
        }

        private static void LimpaDespesaTemporaria(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table cl_despesa_temp;
			");
        }
    }
}
