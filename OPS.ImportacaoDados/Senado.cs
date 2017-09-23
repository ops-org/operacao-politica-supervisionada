using System;
using System.Collections.Generic;
using System.Data;
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
		public static void CarregaSenadores()
		{
			//StringBuilder email = new StringBuilder();

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
									banco.AddParameter("CodigoParlamentar", Convert.ToInt32(senador["CodigoParlamentar"]));
									banco.AddParameter("nome_parlamentar", Convert.ToString(senador["nome_parlamentar"]).ToUpper());
									banco.AddParameter("NomeCompletoParlamentar", Convert.ToString(senador["NomeCompletoParlamentar"]));
									banco.AddParameter("SexoParlamentar", Convert.ToString(senador["SexoParlamentar"])[0].ToString());
									banco.AddParameter("Url", Convert.ToString(senador["UrlPaginaParlamentar"]));
									banco.AddParameter("Foto", Convert.ToString(senador["UrlFotoParlamentar"]));
									banco.AddParameter("SiglaPartido", Convert.ToString(senador["SiglaPartidoParlamentar"]));
									banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
									banco.AddParameter("EmailParlamentar", Convert.ToString(senador["EmailParlamentar"]));
									// Ao invés de gravar o fim do mandato grava o início
									//banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
									banco.ExecuteNonQuery(
										@"INSERT INTO sf_senador (
											id, nome, nome_completo, sexo, url, foto, id_partido, id_estado, email, ativo
										) VALUES (
											@CodigoParlamentar, @nome_parlamentar, @NomeCompletoParlamentar, @SexoParlamentar, @Url, @Foto, 
											(SELECT id FROM partido where sigla like @SiglaPartido), (SELECT id FROM estado where sigla like @SiglaUf), 
											@EmailParlamentar, 'S'
										)");
								}
								catch
								{
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
											nome_completo = @NomeCompletoParlamentar
											, sexo = @SexoParlamentar
											, url = @Url
											, foto = @Foto
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
				Console.WriteLine(ex.Message);
			}
		}

		public static void ImportarDespesas(string atualDir, int ano, bool completo)
		{
			var downloadUrl = string.Format("http://www.senado.gov.br/transparencia/LAI/verba/{0}.csv", ano);
			var fullFileNameCsv = atualDir + @"\" + ano + ".csv";

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
						return;
				}

				using (var client = new WebClient())
				{
					client.Headers.Add("User-Agent: Other");
					client.DownloadFile(downloadUrl, fullFileNameCsv);
				}
			}

			CarregaDadosCsv(fullFileNameCsv, ano, completo);

			//File.Delete(fullFileNameCsv);
		}

		private static void CarregaDadosCsv(string file, int ano, bool completo)
		{
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
				var lstHash = new Dictionary<string, long>();
				using (var dReader = banco.ExecuteReader("select id, hash from sf_despesa where ano=" + ano))
				{
					while (dReader.Read())
					{
						lstHash.Add(dReader["hash"].ToString(), Convert.ToInt64(dReader["id"]));
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
						banco.AddParameter("data", !string.IsNullOrEmpty(valores[DATA]) ? (object)Convert.ToDateTime(valores[DATA]) : DBNull.Value);
						banco.AddParameter("detalhamento", valores[DETALHAMENTO]);
						banco.AddParameter("valor_reembolsado", Convert.ToDouble(valores[VALOR_REEMBOLSADO]));

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

				if (lstHash.Count > 0)
				{
					string lstExcluir = lstHash.Aggregate("", (keyString, pair) => keyString + "," + pair.Value);
					banco.ExecuteNonQuery(string.Format("delete from sf_despesa where id IN({0})", lstExcluir.Substring(1)));
				}

				ProcessarDespesasTemp(banco, completo);
			}

			if (ano == DateTime.Now.Year)
			{
				AtualizaSenadorValores();
				AtualizaCampeoesGastos();
				AtualizaResumoMensal();

				using (var banco = new Banco())
				{
					banco.ExecuteNonQuery(@"
				        UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
			        ");
				}
			}
		}

		private static void ProcessarDespesasTemp(Banco banco, bool completo)
		{
			CorrigeDespesas(banco);
			InsereSenadorFaltante(banco);
			InsereFornecedorFaltante(banco);

			//if (completo)
			InsereDespesaFinal(banco);
			//else
			//	InsereDespesaFinalParcial(banco);

			LimpaDespesaTemporaria(banco);
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

		private static void InsereSenadorFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO sf_senador (nome)
				select distinct senador
				from sf_despesa_temp
				where senador  not in (
					select nome from sf_senador
				);
			");

			if (banco.Rows > 0)
			{
				Console.WriteLine(banco.Rows + " novos senadores!");

				DownloadFotosSenadores(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\SENADOR\");
			}
		}

		private static void InsereFornecedorFaltante(Banco banco)
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
		}

		private static void InsereDespesaFinal(Banco banco)
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
				inner join sf_senador p on p.nome = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);
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
				var dt = banco.GetTable("select id from sf_senador");
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
					where l.ano_mes >= 201502 
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

		public static void DownloadFotosSenadores(string dirRaiz)
		{
			using (var banco = new Banco())
			{
				DataTable table = banco.GetTable("SELECT id, foto FROM sf_senador where foto is not null", 0);

				foreach (DataRow row in table.Rows)
				{
					string id = row["id"].ToString();
					string src = dirRaiz + id + ".jpg";
					if (File.Exists(src)) continue;

					try
					{
						using (WebClient client = new WebClient())
						{
							client.Headers.Add("User-Agent: Other");
							client.DownloadFile(row["foto"].ToString(), src);

							ImportacaoUtils.CreateImageThumbnail(src);

							Console.WriteLine("Atualizado imagem do senador " + id);
						}
					}
					catch (Exception)
					{
						//ignore
					}
				}
			}
		}
	}
}