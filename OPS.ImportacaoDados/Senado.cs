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
                           banco.AddParameter("CodigoParlamentar",
                               Convert.ToInt32(senador["CodigoParlamentar"]));
                           banco.AddParameter("NomeParlamentar",
                               Convert.ToString(senador["NomeParlamentar"]).ToUpper());
                           banco.AddParameter("Url", Convert.ToString(senador["UrlPaginaParlamentar"]));
                           banco.AddParameter("Foto", Convert.ToString(senador["UrlFotoParlamentar"]));
                           banco.AddParameter("SiglaPartido",
                               Convert.ToString(senador["SiglaPartidoParlamentar"]));
                           banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
                           // Ao invés de gravar o fim do mandato grava o início
                           //banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
                           banco.ExecuteNonQuery(
                               "INSERT INTO sf_senador (id, nome, Url, foto, id_partido, id_estado, ativo) VALUES (@CodigoParlamentar, @NomeParlamentar, @Url, @Foto, (SELECT id FROM partido where sigla like @SiglaPartido), (SELECT id FROM estado where sigla like @SiglaUf), 'S')");
                        }
                        catch
                        {
                           banco.AddParameter("Url", Convert.ToString(senador["UrlPaginaParlamentar"]));
                           banco.AddParameter("Foto", Convert.ToString(senador["UrlFotoParlamentar"]));
                           banco.AddParameter("SiglaPartido",
                               Convert.ToString(senador["SiglaPartidoParlamentar"]));
                           banco.AddParameter("SiglaUf", Convert.ToString(senador["UfParlamentar"]));
                           banco.AddParameter("CodigoParlamentar",
                               Convert.ToInt32(senador["CodigoParlamentar"]));
                           // Ao invés de gravar o fim do mandato grava o início
                           //banco.AddParameter("MandatoAtual", Convert.ToDateTime(senador["MandatoAtual"]).AddYears(-9).ToString("yyyyMM"));
                           banco.ExecuteNonQuery(
                               "UPDATE sf_senador SET url = @Url, foto = @Foto, id_partido = (SELECT id FROM partido where sigla like @SiglaPartido), id_estado = (SELECT id FROM estado where sigla like @SiglaUf), ativo = 'S' WHERE id = @CodigoParlamentar");
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
            var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
            long ContentLengthLocal = 0;

            if (File.Exists(fullFileNameCsv))
               ContentLengthLocal = new FileInfo(fullFileNameCsv).Length;

            if (!completo && ContentLength == ContentLengthLocal)
               return;

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
         //try
         //{
         var linhaAtual = 0;

         using (var banco = new Banco())
         {
            //if (!completo)
            //{
            //   banco.ExecuteNonQuery(@"
            //         delete from sf_despesa where ano=2017;

            //         -- select max(id)+1 from sf_despesa;
            //         ALTER TABLE sf_despesa AUTO_INCREMENT = 196405;
            //      ");
            //}

            LimpaDespesaTemporaria(banco);

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            {
               short count = 0;

               while (!reader.EndOfStream)
               {
                  count++;

                  var line = reader.ReadLine();
                  if (string.IsNullOrEmpty(line))
                     continue;
                  if (!line.EndsWith("\""))
                     line += reader.ReadLine();

                  var values = ParseRowToList(line);

                  if (count == 1) //Pula primeira linha
                     continue;

                  if (count == 2)
                  {
                     if ((values[0] != "ANO") ||
                         (values[1] != "MES") ||
                         (values[2] != "SENADOR") ||
                         (values[3] != "TIPO_DESPESA") ||
                         (values[4] != "CNPJ_CPF") ||
                         (values[5] != "FORNECEDOR") ||
                         (values[6] != "DOCUMENTO") ||
                         (values[7] != "DATA") ||
                         (values[8] != "DETALHAMENTO") ||
                         (values[9] != "VALOR_REEMBOLSADO"))
                        return;

                     continue;
                  }

                  banco.AddParameter("ano", Convert.ToInt32(values[0]));
                  banco.AddParameter("mes", Convert.ToInt32(values[1]));
                  banco.AddParameter("senador", values[2]);
                  banco.AddParameter("tipo_despesa", values[3]);
                  banco.AddParameter("cnpj_cpf", !string.IsNullOrEmpty(values[4]) ? Utils.RemoveCaracteresNaoNumericos(values[4]) : "");
                  banco.AddParameter("fornecedor", values[5]);
                  banco.AddParameter("documento", values[6]);
                  banco.AddParameter("data", !string.IsNullOrEmpty(values[7]) ? (object)Convert.ToDateTime(values[7]) : DBNull.Value);
                  banco.AddParameter("detalhamento", values[8]);
                  banco.AddParameter("valor_reembolsado", Convert.ToDouble(values[9]));

                  banco.ExecuteNonQuery(
                      @"INSERT INTO sf_despesa_temp (
								ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, `data`, detalhamento, valor_reembolsado
							) VALUES (
								@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado
							)");
               }

               if (completo && (++linhaAtual == 10000))
               {
                  linhaAtual = 0;

                  ProcessarDespesasTemp(banco, completo);
               }
            }
         }

         using (var banco = new Banco())
         {
            ProcessarDespesasTemp(banco, completo);

            banco.ExecuteNonQuery(@"
				        UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
			        ");
         }

         if (ano == DateTime.Now.Year)
         {
            AtualizaSenadorValores();
         }

         //}
         //catch (Exception ex)
         //{
         //    //var x = ex.Message;
         //}
      }

      private static List<string> ParseRowToList(string row)
      {
         try
         {
            return row.Substring(1, row.Length - 2).Split(new[] { @""";""" }, StringSplitOptions.None).ToList();
         }
         catch
         {
            return null;
         }
      }

      private static void ProcessarDespesasTemp(Banco banco, bool completo)
      {
         CorrigeDespesas(banco);
         InsereSenadorFaltante(banco);
         InsereFornecedorFaltante(banco);

         if (completo)
            InsereDespesaFinal(banco);
         else
            InsereDespesaFinalParcial(banco);

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
   }
}