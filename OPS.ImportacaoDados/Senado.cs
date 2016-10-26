using OPS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OPS.ImportacaoDados
{
	public class Senado
	{
		public static void ImportarDespesas(string atualDir, int ano)
		{
			string downloadUrl = string.Format("http://www.senado.gov.br/transparencia/LAI/verba/{0}.csv", ano);
			string fullFileNameCsv = atualDir + @"\" + ano + ".csv";

			//if (!Directory.Exists(atualDir))
			//{
			//	Directory.CreateDirectory(atualDir);
			//}

			//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);

			//request.UserAgent = "Other";
			//request.Method = "HEAD";
			//request.ContentType = "application/json;charset=UTF-8";
			//request.Timeout = 1000000;

			//using (System.Net.WebResponse resp = request.GetResponse())
			//{
			//	long ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
			//	long ContentLengthLocal = 0;

			//	if (File.Exists(fullFileNameCsv))
			//	{
			//		ContentLengthLocal = new System.IO.FileInfo(fullFileNameCsv).Length;
			//	}

			//	if (ContentLength == ContentLengthLocal)
			//	{
			//		//Do something useful with ContentLength here 
			//		return;
			//	}

			//	using (WebClient client = new WebClient())
			//	{
			//		client.Headers.Add("User-Agent: Other");
			//		client.DownloadFile(downloadUrl, fullFileNameCsv);
			//	}
			//}

			CarregaDadosCsv(fullFileNameCsv);
			
			//File.Delete(fullFileNameCsv);
		}

		private static void CarregaDadosCsv(String file)
		{
			try
			{
				using (Banco banco = new Banco())
				{
					LimpaDespesaTemporaria(banco);

					using (StreamReader reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
					{
						Int16 count = 0;

						while (!reader.EndOfStream)
						{
							count++;

							String line = reader.ReadLine();
							if (string.IsNullOrEmpty(line))
							{
								// Ignorar registros vazios no meio doo arquivo
								continue;
							}
							else if (!line.EndsWith("\""))
							{
								// O arquivo pode vir com quebra de linha quando o texto no detalhamento é muito grande
								line += reader.ReadLine();
							}

							List<String> values = ParseRowToList(line);

							if (count == 1) //Pula primeira linha
								continue;

							if (count == 2)
							{
								if (values[0] != "ANO" ||
									values[1] != "MES" ||
									values[2] != "SENADOR" ||
									values[3] != "TIPO_DESPESA" ||
									values[4] != "CNPJ_CPF" ||
									values[5] != "FORNECEDOR" ||
									values[6] != "DOCUMENTO" ||
									values[7] != "DATA" ||
									values[8] != "DETALHAMENTO" ||
									values[9] != "VALOR_REEMBOLSADO")
								{
									return;
								}

								continue;
							}

							String cnpj;

							banco.AddParameter("ano", Convert.ToInt32(values[0]));
							banco.AddParameter("mes", Convert.ToInt32(values[1]));
							banco.AddParameter("senador", values[2]);
							banco.AddParameter("tipo_despesa", values[3]);
							banco.AddParameter("fornecedor", values[5]);
							banco.AddParameter("documento", values[6]);
							banco.AddParameter("detalhamento", values[8]);
							banco.AddParameter("valor_reembolsado", Convert.ToDouble(values[9]));

							try { banco.AddParameter("data", Convert.ToDateTime(values[7])); }
							catch { banco.AddParameter("data", DBNull.Value); }

							try
							{
								if (values[4].IndexOf("/") > 0)
									cnpj = Convert.ToInt64(values[4].Replace(".", "").Replace("-", "").Replace("/", "")).ToString("00000000000000");
								else
									cnpj = Convert.ToInt64(values[4].Replace(".", "").Replace("-", "")).ToString("00000000000");
							}
							catch
							{
								cnpj = "";
							}

							banco.AddParameter("cnpj_cpf", cnpj);

							banco.ExecuteNonQuery(
								@"INSERT INTO sf_despesa_temp (
								ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, `data`, detalhamento, valor_reembolsado
							) VALUES (
								@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado
							)");
						}

						CorrigeDespesas(banco);
						InsereFornecedorFaltante(banco);
						InsereDespesaFinal(banco);
						LimpaDespesaTemporaria(banco);
					}
				}
			}
			catch (Exception ex)
			{
				var x = 1;
			}
		}

		private static List<String> ParseRowToList(String row)
		{
			try
			{
				return row.Substring(1, row.Length - 2).Split(new string[] { @""";""" }, StringSplitOptions.None).ToList();
			}
			catch (Exception e)
			{
				return null;
			}
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
					valor
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
					d.valor_reembolsado
				FROM sf_despesa_temp d
				inner join sf_senador p on p.nome = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);
		}

		private static void LimpaDespesaTemporaria(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				truncate table sf_despesa_temp;
			");
		}
	}
}
