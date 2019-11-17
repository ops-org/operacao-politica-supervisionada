using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using OPS.Core;

namespace OPS.ImportacaoDados
{
    public class Senado
    {
        public static string CarregaSenadoresAtuais()
        {
            int novos = 0;

            try
            {
                using (var banco = new Banco())
                {
                    banco.ExecuteNonQuery("UPDATE sf_senador SET ativo = 'N'");

                    using (var senado = new DataSet())
                    {
                        senado.ReadXml("http://legis.senado.gov.br/dadosabertos/senador/lista/atual");

                        using (var senadores = senado.Tables["IdentificacaoParlamentar"])
                        {
                            foreach (DataRow senador in senadores.Rows)
                                try
                                {
                                    banco.ClearParameters();
                                    banco.AddParameter("CodigoParlamentar", Convert.ToInt32(senador["CodigoParlamentar"]));
                                    banco.AddParameter("nome_parlamentar", Convert.ToString(senador["NomeParlamentar"]).ToUpper());
                                    banco.AddParameter("NomeCompletoParlamentar", Convert.ToString(senador["NomeCompletoParlamentar"]));
                                    banco.AddParameter("SexoParlamentar", Convert.ToString(senador["SexoParlamentar"])[0].ToString());
                                    banco.AddParameter("SiglaPartido", Convert.ToString(senador["SiglaPartidoParlamentar"]));
                                    banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
                                    banco.AddParameter("EmailParlamentar", Convert.ToString(senador["EmailParlamentar"]));
                                    // Ao invés de gravar o fim do mandato grava o início
                                    //banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
                                    banco.ExecuteNonQuery(
                                        @"INSERT INTO sf_senador (
											id, nome, nome_completo, sexo, id_partido, id_estado, email, ativo
										) VALUES (
											@CodigoParlamentar, @nome_parlamentar, @NomeCompletoParlamentar, @SexoParlamentar,
											(SELECT id FROM partido where sigla like @SiglaPartido), (SELECT id FROM estado where sigla like @SiglaUf), 
											@EmailParlamentar, 'S'
										)");

                                    novos++;

                                }
                                catch
                                {
                                    banco.ClearParameters();
                                    banco.AddParameter("nome_parlamentar", Convert.ToString(senador["NomeParlamentar"]).ToUpper());
                                    banco.AddParameter("NomeCompletoParlamentar", Convert.ToString(senador["NomeCompletoParlamentar"]));
                                    banco.AddParameter("SexoParlamentar", Convert.ToString(senador["SexoParlamentar"])[0].ToString());
                                    banco.AddParameter("Url", Convert.ToString(senador["UrlPaginaParlamentar"]));
                                    banco.AddParameter("Foto", Convert.ToString(senador["UrlFotoParlamentar"]));
                                    banco.AddParameter("SiglaPartido", Convert.ToString(senador["SiglaPartidoParlamentar"]));
                                    banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
                                    banco.AddParameter("EmailParlamentar", Convert.ToString(senador["EmailParlamentar"]));
                                    banco.AddParameter("CodigoParlamentar", Convert.ToInt32(senador["CodigoParlamentar"]));
                                    // Ao invés de gravar o fim do mandato grava o início
                                    //banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
                                    banco.ExecuteNonQuery(
                                        @"UPDATE sf_senador SET 
                                            nome = @nome_parlamentar
											, nome_completo = @NomeCompletoParlamentar
											, sexo = @SexoParlamentar
											, id_partido = (SELECT id FROM partido where sigla like @SiglaPartido)
											, id_estado = (SELECT id FROM estado where sigla like @SiglaUf)
											, email = @EmailParlamentar
											, ativo = 'S' 
										WHERE id = @CodigoParlamentar");
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToFullDescriptionString());
            }

            return novos.ToString() + " Senador(es) Inseridos";
        }

        public static void AtualizaSenadores()
        {

            using (var banco = new Banco())
            {
                DataTable dtSenadores = banco.GetTable("SELECT id FROM sf_senador");

                foreach (DataRow dr in dtSenadores.Rows)
                {
                    try
                    {
                        using (var senado = new DataSet())
                        {
                            senado.ReadXml("http://legis.senado.leg.br/dadosabertos/senador/" + dr["id"].ToString());

                            DataRow senador = senado.Tables["IdentificacaoParlamentar"].Rows[0];
                            DataRow senadosDadosBasicos = senado.Tables["DadosBasicosParlamentar"].Rows[0];

                            banco.AddParameter("NomeParlamentar", Convert.ToString(senador["NomeParlamentar"]));
                            banco.AddParameter("NomeCompletoParlamentar", Convert.ToString(senador["NomeCompletoParlamentar"]));
                            banco.AddParameter("SexoParlamentar", Convert.ToString(senador["SexoParlamentar"])[0].ToString());

                            try
                            {
                                banco.AddParameter("Url", Convert.ToString(senador["UrlPaginaParlamentar"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("Url", DBNull.Value);
                            }

                            try
                            {
                                banco.AddParameter("Foto", Convert.ToString(senador["UrlFotoParlamentar"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("Foto", DBNull.Value);
                            }

                            banco.AddParameter("SiglaPartido", Convert.ToString(senador["SiglaPartidoParlamentar"]));
                            try
                            {
                                banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    banco.AddParameter("SiglaUf", Convert.ToString(senadosDadosBasicos["UfNaturalidade"]));
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        DataRow ultimoMandato = senado.Tables["UltimoMandato"].Rows[0];
                                        banco.AddParameter("SiglaUf", Convert.ToString(ultimoMandato["UfParlamentar"]));
                                    }
                                    catch (Exception)
                                    {
                                        banco.AddParameter("SiglaUf", DBNull.Value);
                                    }
                                }
                            }

                            try
                            {
                                banco.AddParameter("EmailParlamentar", Convert.ToString(senador["EmailParlamentar"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("EmailParlamentar", DBNull.Value);
                            }


                            banco.AddParameter("DataNascimento", Convert.ToString(senadosDadosBasicos["DataNascimento"]));
                            banco.AddParameter("CodigoParlamentar", Convert.ToInt32(senador["CodigoParlamentar"]));
                            // Ao invés de gravar o fim do mandato grava o início
                            //banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
                            banco.ExecuteNonQuery(
                                @"UPDATE sf_senador SET 
											nome = @NomeParlamentar
											, nome_completo = @NomeCompletoParlamentar
											, sexo = @SexoParlamentar
											, url = @Url
											, foto = @Foto
											, id_partido = (SELECT id FROM partido where sigla like @SiglaPartido)
											, id_estado = (SELECT id FROM estado where sigla like @SiglaUf)
											, email = @EmailParlamentar
											, nascimento = @DataNascimento
										WHERE id = @CodigoParlamentar");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToFullDescriptionString());
                    }
                }
            }
        }


        public static void CarregaSenadores()
        {

            using (var banco = new Banco())
            {
                for (int i = 4921; i <= 6000; i++)
                {
                    try
                    {
                        using (var senado = new DataSet())
                        {
                            senado.ReadXml("http://legis.senado.leg.br/dadosabertos/senador/" + i.ToString());

                            DataRow senador = senado.Tables["IdentificacaoParlamentar"].Rows[0];
                            DataRow senadosDadosBasicos = senado.Tables["DadosBasicosParlamentar"].Rows[0];

                            banco.ClearParameters();
                            banco.AddParameter("CodigoParlamentar", Convert.ToInt32(senador["CodigoParlamentar"]));

                            try
                            {
                                banco.AddParameter("NomeParlamentar", Convert.ToString(senador["NomeParlamentar"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("NomeParlamentar", Convert.ToString(senador["NomeCompletoParlamentar"]));
                            }

                            banco.AddParameter("NomeCompletoParlamentar", Convert.ToString(senador["NomeCompletoParlamentar"]));
                            banco.AddParameter("SexoParlamentar", Convert.ToString(senador["SexoParlamentar"])[0].ToString());

                            try
                            {
                                banco.AddParameter("SiglaPartido", Convert.ToString(senador["SiglaPartidoParlamentar"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("SiglaPartido", DBNull.Value);
                            }

                            try
                            {
                                banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    banco.AddParameter("SiglaUf", Convert.ToString(senadosDadosBasicos["UfNaturalidade"]));
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        DataRow ultimoMandato = senado.Tables["UltimoMandato"].Rows[0];
                                        banco.AddParameter("SiglaUf", Convert.ToString(ultimoMandato["UfParlamentar"]));
                                    }
                                    catch (Exception)
                                    {
                                        banco.AddParameter("SiglaUf", DBNull.Value);
                                    }
                                }
                            }

                            try
                            {
                                banco.AddParameter("EmailParlamentar", Convert.ToString(senador["EmailParlamentar"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("EmailParlamentar", DBNull.Value);
                            }

                            try
                            {
                                banco.AddParameter("DataNascimento", Convert.ToString(senadosDadosBasicos["DataNascimento"]));
                            }
                            catch (Exception)
                            {
                                banco.AddParameter("DataNascimento", DBNull.Value);
                            }

                            // Ao invés de gravar o fim do mandato grava o início
                            //banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
                            banco.ExecuteNonQuery(
                                @"INSERT INTO sf_senador (
									id,
									nome,
									nome_completo,
									sexo,
									id_partido,
									id_estado,
									email,
									nascimento,
									ativo
								) values (
									@CodigoParlamentar
									, @NomeParlamentar
									, @NomeCompletoParlamentar
									, @SexoParlamentar
									, (SELECT id FROM partido where sigla like @SiglaPartido)
									, (SELECT id FROM estado where sigla like @SiglaUf)
									, @EmailParlamentar
									, @DataNascimento
									, 'N'
								)");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is NullReferenceException))
                        {
                            Console.WriteLine(ex.ToFullDescriptionString());
                        }
                    }
                }
            }
        }

        public static string ImportarDespesas(string atualDir, int ano, bool completo)
        {
            var downloadUrl = string.Format("http://www.senado.gov.br/transparencia/LAI/verba/{0}.csv", ano);
            var fullFileNameCsv = System.IO.Path.Combine(atualDir, ano + ".csv");

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
                var resumoImportacao = CarregaDadosCsv(fullFileNameCsv, ano, completo);

                using (var banco = new Banco())
                {
                    banco.ExecuteNonQuery(@"
					UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
				");
                }

                return resumoImportacao;
            }
            catch (Exception ex)
            {
                // Excluir o arquivo para tentar importar novamente na proxima execução
                File.Delete(fullFileNameCsv);

                return "Erro ao importar:" + ex.ToFullDescriptionString();
            }
        }

        private static string CarregaDadosCsv(string file, int ano, bool completo)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var sb = new StringBuilder();
            string sResumoValores = string.Empty;

            int indice = 0;
            int ANO = indice++;
            int MES = indice++;
            int SENADOR = indice++;
            int TIPO_DESPESA = indice++;
            int CNPJ_CPF = indice++;
            int FORNECEDOR = indice++;
            int DOCUMENTO = indice++;
            int DATA = indice++;
            int DETALHAMENTO = indice++;
            int VALOR_REEMBOLSADO = indice++;

            int linhaAtual = 0;

            using (var banco = new Banco())
            {
                var lstHash = new List<string>();
                using (var dReader = banco.ExecuteReader("select hash from sf_despesa where ano=" + ano))
                {
                    while (dReader.Read())
                    {
                        try
                        {
                            lstHash.Add(dReader["hash"].ToString());
                        }
                        catch (Exception)
                        {
                            // Vai ter duplicado mesmo
                        }
                    }
                }

                using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens from sf_despesa where ano=" + ano))
                {
                    if (dReader.Read())
                    {
                        sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                    }
                }

                LimpaDespesaTemporaria(banco);

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short count = 0;

                    while (!reader.EndOfStream)
                    {
                        count++;

                        var linha = reader.ReadLine();
                        if (string.IsNullOrEmpty(linha))
                            continue;
                        if (!linha.EndsWith("\""))
                            linha += reader.ReadLine();

                        var valores = ImportacaoUtils.ParseCsvRowToList(@""";""", linha);

                        if (count == 1) //Pula primeira linha, data da atualização
                            continue;

                        if (count == 2)
                        {
                            if (
                                    (valores[ANO] != "ANO") ||
                                    (valores[MES] != "MES") ||
                                    (valores[SENADOR] != "SENADOR") ||
                                    (valores[TIPO_DESPESA] != "TIPO_DESPESA") ||
                                    (valores[CNPJ_CPF] != "CNPJ_CPF") ||
                                    (valores[FORNECEDOR] != "FORNECEDOR") ||
                                    (valores[DOCUMENTO] != "DOCUMENTO") ||
                                    (valores[DATA] != "DATA") ||
                                    (valores[DETALHAMENTO] != "DETALHAMENTO") ||
                                    (valores[VALOR_REEMBOLSADO] != "VALOR_REEMBOLSADO")
                                )
                            {
                                throw new Exception("Mudança de integração detectada para o Senado Federal");
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        banco.AddParameter("ano", Convert.ToInt32(valores[ANO]));
                        banco.AddParameter("mes", Convert.ToInt32(valores[MES]));
                        banco.AddParameter("senador", valores[SENADOR]);
                        banco.AddParameter("tipo_despesa", valores[TIPO_DESPESA]);
                        banco.AddParameter("cnpj_cpf", !string.IsNullOrEmpty(valores[CNPJ_CPF]) ? Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF]) : "");
                        banco.AddParameter("fornecedor", valores[FORNECEDOR]);
                        banco.AddParameter("documento", valores[DOCUMENTO]);
                        banco.AddParameter("data", !string.IsNullOrEmpty(valores[DATA]) ? (object)Convert.ToDateTime(valores[DATA], cultureInfo) : DBNull.Value);
                        banco.AddParameter("detalhamento", valores[DETALHAMENTO]);
                        banco.AddParameter("valor_reembolsado", Convert.ToDouble(valores[VALOR_REEMBOLSADO], cultureInfo));

                        string hash = banco.ParametersHash();
                        if (lstHash.Remove(hash))
                        {
                            banco.ClearParameters();
                            continue;
                        }

                        banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO sf_despesa_temp (
								ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, `data`, detalhamento, valor_reembolsado, hash
							) VALUES (
								@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado, @hash
							)");
                    }

                    if (++linhaAtual % 100 == 0)
                    {
                        Console.WriteLine(linhaAtual);
                    }
                }

                if (lstHash.Count == 0 && linhaAtual == 0)
                {
                    sb.AppendFormat("<p>Não há novos itens para importar! #2</p>");
                    return sb.ToString();
                }

                if (lstHash.Count > 0)
                {
                    foreach (var hash in lstHash)
                    {
                        banco.ExecuteNonQuery(string.Format("delete from sf_despesa where hash = '{0}'", hash));
                    }
                    

                    Console.WriteLine("Registros para exluir: " + lstHash.Count);
                    sb.AppendFormat("<p>{0} registros excluidos</p>", lstHash.Count);
                }

                sb.Append(ProcessarDespesasTemp(banco, completo));
            }

            if (ano == DateTime.Now.Year)
            {
                AtualizaSenadorValores();
                AtualizaCampeoesGastos();
                AtualizaResumoMensal();
            }

            using (var banco = new Banco())
            {
                using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens from sf_despesa where ano=" + ano))
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

        private static string ProcessarDespesasTemp(Banco banco, bool completo)
        {
            var sb = new StringBuilder();

            CorrigeDespesas(banco);
            sb.Append(InsereSenadorFaltante(banco));
            sb.Append(InsereFornecedorFaltante(banco));

            //if (completo)
            sb.Append(InsereDespesaFinal(banco));
            //else
            //	InsereDespesaFinalParcial(banco);

            LimpaDespesaTemporaria(banco);

            return sb.ToString();
        }

        private static void CorrigeDespesas(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				UPDATE sf_despesa_temp 
				SET tipo_despesa = 'Aquisição de material de consumo para uso no escritório político' 
				WHERE tipo_despesa LIKE 'Aquisição de material de consumo para uso no escritório político%';

				UPDATE sf_despesa_temp 
				SET tipo_despesa = 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços' 
				WHERE tipo_despesa LIKE 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços%';	
			");
        }

        private static string InsereSenadorFaltante(Banco banco)
        {
            //object total = banco.ExecuteScalar(@"select count(1) from sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
            //if (Convert.ToInt32(total) > 0)
            //{
            //	CarregaSenadoresAtuais();

            object total = banco.ExecuteScalar(@"select count(1) from sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
            if (Convert.ToInt32(total) > 0)
            {
                throw new Exception("Existem despesas de senadores que não estão cadastrados!");
            }
            //}

            return string.Empty;
        }

        private static string InsereFornecedorFaltante(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.fornecedor), dt.cnpj_cpf
				from sf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				where dt.cnpj_cpf is not null
				and f.id is null
				GROUP BY dt.cnpj_cpf;
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
				ALTER TABLE sf_despesa DISABLE KEYS;

				INSERT INTO sf_despesa (
					id_sf_senador,
					id_sf_despesa_tipo,
					id_fornecedor,
					ano_mes,
					ano,
					mes,
					documento,
					data_documento,
					detalhamento,
					valor,
					hash
				)
				SELECT 
					p.id,
					dt.id,
					f.id,
					concat(ano, LPAD(mes, 2, '0')),
					d.ano,
					d.mes,
					d.documento,
					d.`data`,
					d.detalhamento,
					d.valor_reembolsado,
					d.hash
				FROM sf_despesa_temp d
				inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Despesa</p>";
            }

            return string.Empty;
        }

        private static void InsereDespesaFinalParcial(Banco banco)
        {
            var dt = banco.GetTable(
                @"DROP TABLE IF EXISTS table_in_memory_d;
                CREATE TEMPORARY TABLE table_in_memory_d
                AS (
	                select id_sf_senador, mes, sum(valor) as total
	                from sf_despesa d
	                where ano = 2017
	                GROUP BY id_sf_senador, mes
                );

                DROP TABLE IF EXISTS table_in_memory_dt;
                CREATE TEMPORARY TABLE table_in_memory_dt
                AS (
	                select p.id as id_sf_senador, mes, sum(valor_reembolsado) as total
	                from sf_despesa_temp d
                    inner join sf_senador p on p.nome = d.senador
	                GROUP BY p.id, mes
                );

                select dt.id_sf_senador, dt.mes
                from table_in_memory_dt dt
                left join table_in_memory_d d on dt.id_sf_senador = d.id_sf_senador and dt.mes = d.mes
                where (d.id_sf_senador is null or d.total <> dt.total)
                order by d.id_sf_senador, d.mes;
			    ", 3600);

            foreach (DataRow dr in dt.Rows)
            {
                banco.AddParameter("id_sf_senador", dr["id_sf_senador"]);
                banco.AddParameter("mes", dr["mes"]);
                banco.ExecuteNonQuery(@"DELETE FROM sf_despesa WHERE id_sf_senador=@id_sf_senador and mes=@mes");

                banco.AddParameter("id_sf_senador", dr["id_sf_senador"]);
                banco.AddParameter("mes", dr["mes"]);
                banco.ExecuteNonQuery(@"
				        INSERT INTO sf_despesa (
					        id_sf_senador,
					        id_sf_despesa_tipo,
					        id_fornecedor,
					        ano_mes,
					        ano,
					        mes,
					        documento,
					        data_documento,
					        detalhamento,
					        valor
				        )
				        SELECT 
					        d.id,
					        dt.id,
					        f.id,
					        concat(ano, LPAD(mes, 2, '0')),
					        d.ano,
					        d.mes,
					        d.documento,
					        d.`data`,
					        d.detalhamento,
					        d.valor_reembolsado
					    from (
						    select d.*, p.id					        
						    from sf_despesa_temp d
                            inner join sf_senador p on p.nome = d.senador
						    WHERE p.id=@id_sf_senador and mes=@mes
					    ) d
				        inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				        inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
			        ", 3600);
            }
        }

        private static void LimpaDespesaTemporaria(Banco banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table sf_despesa_temp;
			");
        }

        public static void AtualizaSenadorValores()
        {
            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery("UPDATE sf_senador SET valor_total_ceaps=0;");

                var dt = banco.GetTable(@"select id from sf_senador
						WHERE id IN (
						select distinct id_sf_senador
						from sf_despesa
					)");
                object valor_total_ceaps;

                foreach (DataRow dr in dt.Rows)
                {
                    banco.AddParameter("id_sf_senador", dr["id"]);
                    valor_total_ceaps =
                        banco.ExecuteScalar("select sum(valor) from sf_despesa where id_sf_senador=@id_sf_senador;");

                    banco.AddParameter("valor_total_ceaps", valor_total_ceaps);
                    banco.AddParameter("id_sf_senador", dr["id"]);
                    banco.ExecuteNonQuery(
                        @"update sf_senador set 
						valor_total_ceaps=@valor_total_ceaps
						where id=@id_sf_senador"
                    );
                }
            }
        }

        public static void AtualizaCampeoesGastos()
        {
            var strSql =
                @"truncate table sf_senador_campeao_gasto;
				insert into sf_senador_campeao_gasto
				SELECT l1.id_sf_senador, d.nome, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_sf_senador,
						sum(l.valor) as valor_total
					FROM  sf_despesa l
					where l.ano_mes >= 201902 
					GROUP BY l.id_sf_senador
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN sf_senador d on d.id = l1.id_sf_senador
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;";

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        public static void AtualizaResumoMensal()
        {
            var strSql =
                @"truncate table sf_despesa_resumo_mensal;
					insert into sf_despesa_resumo_mensal
					(ano, mes, valor) (
						select ano, mes, sum(valor)
						from sf_despesa
						group by ano, mes
					);";

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        public static string DownloadFotosSenadores(string dirRaiz)
        {
            var db = new StringBuilder();

            using (var banco = new Banco())
            {
                DataTable table = banco.GetTable("SELECT id FROM sf_senador where valor_total_ceaps > 0 or ativo = 'S'");

                foreach (DataRow row in table.Rows)
                {
                    string id = row["id"].ToString();
                    string url = "https://www.senado.leg.br/senadores/img/fotos-oficiais/senador" + id + ".jpg";
                    string src = dirRaiz + id + ".jpg";
                    if (File.Exists(src)) continue;

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add("User-Agent: Other");
                            client.DownloadFile(url, src);

                            ImportacaoUtils.CreateImageThumbnail(src, 120, 160);
                            ImportacaoUtils.CreateImageThumbnail(src, 240, 300);

                            db.AppendLine("Atualizado imagem do senador " + id);
                            Console.WriteLine("Atualizado imagem do senador " + id);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("404"))
                        {
                            db.AppendLine("Imagem do senador " + id + " inexistente! Motivo: " + ex.ToFullDescriptionString());
                            Console.WriteLine("Imagem do senador " + id + " inexistente! Motivo: " + ex.ToFullDescriptionString());
                            //ignore
                        }
                    }
                }
            }

            return db.ToString();
        }
    }
}