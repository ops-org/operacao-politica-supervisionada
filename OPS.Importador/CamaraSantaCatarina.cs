using CsvHelper;
using OPS.Core;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OPS.Importador
{
    public static class CamaraSantaCatarina
    {
        public static string ImportarDespesas(string atualDir, int ano, bool completo)
        {
            string downloadUrl = string.Format("http://transparencia.alesc.sc.gov.br/gabinetes_csv.php?ano={0}", ano);

            var fullFileNameCsv = atualDir + @"\CLSC_" + ano + ".csv";

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

                if (!File.Exists(fullFileNameCsv))
                {
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("User-Agent: Other");
                        client.DownloadFile(downloadUrl, fullFileNameCsv);
                    }
                }
            }

            try
            {
                var resumoImportacao = CarregaDadosCsv(fullFileNameCsv, ano);

                //using (var banco = new Banco())
                //{
                //    banco.ExecuteNonQuery(@"
                //	    UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
                //    ");
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

        private static string CarregaDadosCsv(string file, int ano)
        {
            var sb = new StringBuilder();
            string sResumoValores = string.Empty;

            int indice = 0;
            int Verba = indice++;
            int Descricao = indice++;
            int Conta = indice++;
            int Favorecido = indice++;
            int Trecho = indice++;
            int Vencimento = indice++;
            int Valor = indice++;

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

                //LimpaDespesaTemporaria(banco);

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

                                if (
                                    (csv[Verba] != "Verba") ||
                                    (csv[Descricao] != "Descrição") ||
                                    (csv[Conta] != "Conta") ||
                                    (csv[Favorecido] != "Favorecido") ||
                                    (csv[Trecho] != "Trecho") ||
                                    (csv[Vencimento] != "Vencimento") ||
                                    (csv[Valor] != "Valor")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }

                                // Pular linha de titulo
                                continue;
                            }

                            if (string.IsNullOrEmpty(csv[Verba])) continue; //Linha vazia


                            banco.AddParameter("Verba", csv[Verba]);
                            banco.AddParameter("Descricao", csv[Descricao]);
                            banco.AddParameter("Conta", csv[Conta]);
                            banco.AddParameter("Favorecido", csv[Favorecido]);
                            banco.AddParameter("Trecho", csv[Trecho]);
                            
                            banco.AddParameter("Vencimento", DateTime.Parse(csv[Vencimento]));

                            string valorTmp = csv[Valor];
                            // Valor 1.500.00 é na verdade 1.500,00
                            Regex myRegex = new Regex(@"\.(\d\d$)", RegexOptions.Singleline);
                            if (myRegex.IsMatch(valorTmp))
                            {
                                valorTmp = myRegex.Replace(valorTmp, @",$1");
                            }

                            banco.AddParameter("Valor", !string.IsNullOrEmpty(valorTmp) ? (object)Convert.ToDouble(valorTmp) : 0);


                            //string hash = banco.ParametersHash();
                            //if (lstHash.Remove(hash))
                            //{
                            //    banco.ClearParameters();
                            //    continue;
                            //}

                            //banco.AddParameter("hash", hash);

                            banco.AddParameter("Ano", ano);

                            banco.ExecuteNonQuery(
                                @"INSERT INTO cl_despesa_temp (
								    tipo_verba, tipo_despesa, nome, favorecido, observacao, data_emissao, valor, ano
							    ) VALUES (
								    @Verba, @Descricao, @Conta, @Favorecido, @Trecho, @Vencimento, @Valor, @Ano
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

        private static string ProcessarDespesasTemp(AppDb banco)
        {
            var sb = new StringBuilder();

            sb.Append(InsereTipoDespesaFaltante(banco));
            sb.Append(InsereDeputadoFaltante(banco));
            //sb.Append(InsereFornecedorFaltante(banco));
            sb.Append(InsereDespesaFinal(banco));
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
        	        INSERT INTO cl_deputado (nome_parlamentar, id_estado)
        	        select distinct Nome, 42
        	        from cl_despesa_temp
        	        where Nome not in (
        		        select Nome from cl_deputado where id_estado = 42
        	        );
                ");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Deputado</p>";
            }

            return string.Empty;
        }

        //private static string InsereFornecedorFaltante(Banco banco)
       // {
       //     banco.ExecuteNonQuery(@"
				   // INSERT INTO fornecedor (nome, cnpj_cpf)
				   // select MAX(dt.empresa), dt.cnpj_cpf
				   // from cl_despesa_temp dt
				   // left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				   // where dt.cnpj_cpf is not null
				   // and f.id is null
       //             -- and LENGTH(dt.cnpj_cpf) <= 14
				   // GROUP BY dt.cnpj_cpf;
			    //");

       //     if (banco.Rows > 0)
       //     {
       //         return "<p>" + banco.Rows + "+ Fornecedor</p>";
       //     }

       //     return string.Empty;
       // }

        private static string InsereDespesaFinal(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
                -- UPDATE cl_despesa_temp SET observacao = NULL WHERE observacao = 'não consta documento';

				INSERT INTO cl_despesa (
					id_cl_deputado,
                    id_cl_despesa_tipo,
	                id_fornecedor,
	                data,
	                ano_mes,
	                numero_documento,
	                valor,
                    favorecido,
                    observacao,
                    hash
				)
                SELECT 
	                p.id AS id_cl_deputado,
                    dts.id_cl_despesa_tipo,
                    IFNULL(f.id, 82624) AS id_fornecedor,
                    d.data_emissao,
                    concat(year(d.data_emissao), LPAD(month(d.data_emissao), 2, '0')) AS ano_mes,
                    d.documento AS numero_documento,
                    d.valor AS valor,
                    d.favorecido,
                    d.observacao AS observacao,
                    null -- d.hash
                FROM cl_despesa_temp d
                inner join cl_deputado p on p.nome_parlamentar like d.nome and id_estado = 42
                left join cl_despesa_tipo_sub dts on dts.descricao = d.tipo_despesa
                LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
                ORDER BY d.id;
			", 3600);

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
