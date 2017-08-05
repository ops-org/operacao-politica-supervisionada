using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OPS.Core.DTO;

namespace OPS.Core.DAO
{
	public class DeputadoDao
	{
		public dynamic Consultar(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT 
						d.id as id_cf_deputado
						, d.id_cadastro
						, d.id_parlamentar
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
						, d.cod_orcamento
						, d.nome_parlamentar
						, d.nome_civil
						, d.condicao
						, d.url_foto
						, d.sexo
						, d.gabinete
						, d.anexo
						, d.fone
						, d.email
						, d.profissao
						, d.nascimento
						, d.falecimento
						, d.matricula
						, d.valor_total_ceap
						, d.quantidade_secretarios
						, g.id as id_cf_gabinete
					FROM cf_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					LEFT JOIN cf_gabinete g on g.id_cadastro_deputado = d.id_cadastro
					WHERE d.id = @id
				");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.Read())
					{
						return new
						{
							id_cf_deputado = reader["id_cf_deputado"],
							id_cadastro = reader["id_cadastro"],
							id_parlamentar = reader["id_parlamentar"],
							id_partido = reader["id_partido"],
							sigla_estado = reader["sigla_estado"].ToString(),
							nome_partido = reader["nome_partido"].ToString(),
							id_estado = reader["id_cadastro"],
							sigla_partido = reader["sigla_partido"].ToString(),
							nome_estado = reader["nome_estado"].ToString(),
							cod_orcamento = reader["cod_orcamento"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
							nome_civil = reader["nome_civil"].ToString(),
							condicao = reader["condicao"].ToString(),
							sexo = reader["sexo"].ToString(),
							gabinete = reader["gabinete"],
							anexo = reader["anexo"].ToString(),
							fone = reader["fone"].ToString(),
							email = reader["email"].ToString(),
							profissao = reader["profissao"].ToString(),
							nascimento = Utils.FormataData(reader["nascimento"]),
							falecimento = Utils.FormataData(reader["falecimento"]),
							matricula = reader["matricula"],
							valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"]),
							id_cf_gabinete = reader["id_cf_gabinete"],
							quantidade_secretarios = reader["quantidade_secretarios"].ToString(),
						};
					}

					return null;
				}
			}
		}

		public dynamic MaioresFornecedores(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();

				strSql.AppendLine(@"
					SELECT
						 pj.id as id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.valor_total
					from(
						SELECT
						sum(l.valor_liquido) as valor_total
						, l.id_fornecedor
						FROM cf_despesa l
						WHERE l.id_cf_deputado = @id
						GROUP BY l.id_fornecedor
						order by 1 desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor_total desc
				");

				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_fornecedor = reader["id_fornecedor"].ToString(),
							cnpj_cpf = reader["cnpj_cpf"].ToString(),
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					return lstRetorno;
				}
			}
		}

		public dynamic MaioresNotas(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();

				strSql.AppendLine(@"
					SELECT
						 l1.id as id_cf_despesa
						, l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.valor_liquido
					from (
						SELECT
						l.id
						, l.valor_liquido
						, l.id_fornecedor
						FROM cf_despesa l
						WHERE l.id_cf_deputado = @id
						order by l.valor_liquido desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor_liquido desc 
				");

				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cf_despesa = reader["id_cf_despesa"].ToString(),
							id_fornecedor = reader["id_fornecedor"].ToString(),
							cnpj_cpf = reader["cnpj_cpf"].ToString(),
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							valor_liquido = Utils.FormataValor(reader["valor_liquido"])
						});
					}

					return lstRetorno;
				}
			}
		}

		public dynamic Documento(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();

				strSql.AppendLine(@"
					SELECT
						l.id as id_cf_despesa
						, l.id_documento
						, l.numero_documento
						, l.tipo_documento
						, l.data_emissao
						, l.valor_documento
						, l.valor_glosa
						, l.valor_liquido
						, l.valor_restituicao
						, l.parcela
						, l.nome_passageiro
						, l.trecho_viagem
						, l.ano
						, l.mes
						, td.id as id_cf_despesa_tipo
						, td.descricao as descricao_despesa
						, d.id as id_cf_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, pj.id AS id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
					FROM cf_despesa l
					LEFT JOIN fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN cf_deputado d ON d.id = l.id_cf_deputado
					LEFT JOIN cf_despesa_tipo td ON td.id = l.id_cf_despesa_tipo
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					WHERE l.id = @id
				 ");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.Read())
					{
						string sTipoDocumento = "";
						switch (reader["tipo_documento"].ToString())
						{
							case "0": sTipoDocumento = "Nota Fiscal"; break;
							case "1": sTipoDocumento = "Recibo"; break;
							case "2": case "3": sTipoDocumento = "Despesa no Exterior"; break;
						}
						var result = new
						{
							id_cf_despesa = reader["id_cf_despesa"],
							id_documento = reader["id_documento"],
							numero_documento = reader["numero_documento"].ToString(),
							tipo_documento = sTipoDocumento,
							data_emissao = Utils.FormataData(reader["data_emissao"]),
							valor_documento = Utils.FormataValor(reader["valor_documento"]),
							valor_glosa = Utils.FormataValor(reader["valor_glosa"]),
							valor_liquido = Utils.FormataValor(reader["valor_liquido"]),
							valor_restituicao = Utils.FormataValor(reader["valor_restituicao"]),
							parcela = reader["parcela"].ToString(),
							nome_passageiro = reader["nome_passageiro"].ToString(),
							trecho_viagem = reader["trecho_viagem"].ToString(),
							ano = reader["ano"].ToString(),
							mes = reader["mes"].ToString(),
							id_cf_despesa_tipo = reader["id_cf_despesa_tipo"],
							descricao_despesa = reader["descricao_despesa"].ToString(),
							id_cf_deputado = reader["id_cf_deputado"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
							sigla_estado = reader["sigla_estado"].ToString(),
							sigla_partido = reader["sigla_partido"].ToString(),
							id_fornecedor = reader["id_fornecedor"],
							cnpj_cpf = reader["cnpj_cpf"].ToString(),
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							competencia = string.Format("{0:00}/{1:0000}", reader["mes"], reader["ano"])
						};

						return result;
					}

					return null;
				}
			}
		}

		public dynamic GastosMensaisPorAno(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT d.ano, d.mes, SUM(d.valor_liquido) AS valor_total
					FROM cf_despesa d
					WHERE d.id_cf_deputado = @id
					group by d.ano, d.mes
					order by d.ano, d.mes
				");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					var lstValoresMensais = new decimal?[12];
					string anoControle = string.Empty;
					bool existeGastoNoAno = false;

					while (reader.Read())
					{
						if (reader["ano"].ToString() != anoControle)
						{
							if (existeGastoNoAno)
							{
								lstRetorno.Add(new
								{
									name = anoControle.ToString(),
									data = lstValoresMensais
								});

								lstValoresMensais = new decimal?[12];
								existeGastoNoAno = false;
							}

							anoControle = reader["ano"].ToString();
						}

						if (Convert.ToDecimal(reader["valor_total"]) > 0)
						{
							lstValoresMensais[Convert.ToInt32(reader["mes"]) - 1] = Convert.ToDecimal(reader["valor_total"]);
							existeGastoNoAno = true;
						}
					}

					if (existeGastoNoAno)
					{
						lstRetorno.Add(new
						{
							name = anoControle.ToString(),
							data = lstValoresMensais
						});
					}

					return lstRetorno;
					// Ex: [{"$id":"1","name":"2015","data":[null,18404.57,25607.82,29331.99,36839.82,24001.68,40811.97,33641.20,57391.30,60477.07,90448.58,13285.14]}]
				}
			}
		}

		public dynamic Pesquisa()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT 
						id, nome_civil, nome_parlamentar 
					FROM cf_deputado 
					ORDER BY nome_parlamentar
				");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader["id"].ToString(),
							tokens = new[] { reader["nome_civil"].ToString(), reader["nome_parlamentar"].ToString() },
							text = reader["nome_parlamentar"].ToString()
						});
					}
				}
				return lstRetorno;
			}
		}

		public dynamic Lancamentos(FiltroParlamentarDTO filtro)
		{
			if (filtro == null) throw new BusinessException("Parâmetro filtro não informado!");

			switch (filtro.Agrupamento)
			{
				case eAgrupamentoAuditoria.Parlamentar:
					return LancamentosParlamentar(filtro);
				case eAgrupamentoAuditoria.Despesa:
					return LancamentosDespesa(filtro);
				case eAgrupamentoAuditoria.Fornecedor:
					return LancamentosFornecedor(filtro);
				case eAgrupamentoAuditoria.Partido:
					return LancamentosPartido(filtro);
				case eAgrupamentoAuditoria.Uf:
					return LancamentosEstado(filtro);
				case eAgrupamentoAuditoria.Documento:
					return LancamentosNotaFiscal(filtro);
			}

			throw new BusinessException("Parâmetro filtro.Agrupamento não informado!");
		}

		private dynamic LancamentosParlamentar(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();

				sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					SELECT
						count(l.id) AS total_notas
					, sum(l.valor_liquido) as valor_total
					, l.id_cf_deputado
					FROM cf_despesa l
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
					GROUP BY id_cf_deputado;
					
					SELECT
						 d.id as id_cf_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, l1.total_notas
						, l1.valor_total
						from table_in_memory l1
					LEFT JOIN cf_deputado d on d.id = l1.id_cf_deputado
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
				");

				AdicionaResultadoComum(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cf_deputado = reader["id_cf_deputado"],
							nome_parlamentar = reader["nome_parlamentar"],
							sigla_estado = reader["sigla_estado"],
							sigla_partido = reader["sigla_partido"],
							total_notas = reader["total_notas"],
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();
					string ValorTotal = Utils.FormataValor(reader[1]);

					return new
					{
						total_count = TotalCount,
						valor_total = ValorTotal,
						results = lstRetorno
					};
				}
			}
		}

		private dynamic LancamentosFornecedor(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();

				sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					SELECT
						l.id_fornecedor
						, count(l.id) AS total_notas
						, sum(l.valor_liquido) as valor_total
					FROM cf_despesa l
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
					GROUP BY l.id_fornecedor;

					select
						l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.total_notas
						, l1.valor_total
					from table_in_memory l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
				");

				AdicionaResultadoComum(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							//SgUf = reader[SgUfOrdinal],
							//DataUltimaNotaFiscal = Utils.FormataData(reader[DataUltimaNotaFiscalOrdinal]),
							//Doador = reader[DoadorOrdinal],
							id_fornecedor = reader["id_fornecedor"],
							cnpj_cpf = reader["cnpj_cpf"],
							nome_fornecedor = reader["nome_fornecedor"],
							total_notas = reader["total_notas"],
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();
					string ValorTotal = Utils.FormataValor(reader[1]);

					return new
					{
						total_count = TotalCount,
						valor_total = ValorTotal,
						results = lstRetorno
					};
				}
			}
		}

		private dynamic LancamentosDespesa(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();

				sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					SELECT
						count(l.id) AS total_notas
						, sum(l.valor_liquido) as valor_total
						, l.id_cf_despesa_tipo
					FROM cf_despesa l
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
					GROUP BY id_cf_despesa_tipo;
					
					SELECT
						l1.id_cf_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					from table_in_memory l1
					LEFT JOIN cf_despesa_tipo td on td.id = l1.id_cf_despesa_tipo
				");

				AdicionaResultadoComum(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cf_despesa_tipo = reader["id_cf_despesa_tipo"],
							descricao = reader["descricao"],
							total_notas = reader["total_notas"],
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();
					string ValorTotal = Utils.FormataValor(reader[1]);

					return new
					{
						total_count = TotalCount,
						valor_total = ValorTotal,
						results = lstRetorno
					};
				}
			}
		}

		private dynamic LancamentosPartido(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();

				sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					AS (
						SELECT
						 d.id_partido
						, p.nome as nome_partido
						, count(l1.total_notas) as total_notas
						, sum(l1.valor_total) as valor_total
						from (
							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor_liquido) as valor_total
							, l.id_cf_deputado
							FROM cf_despesa l
							WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
						GROUP BY id_cf_deputado
					) l1
					INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado
					LEFT JOIN partido p on p.id = d.id_partido
					GROUP BY p.id, p.nome
				);"); //end table_in_memory

				sqlSelect.AppendLine("select * from table_in_memory ");
				AdicionaResultadoComum(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_partido = reader["id_partido"],
							nome_partido = reader["nome_partido"],
							total_notas = reader["total_notas"],
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();
					string ValorTotal = Utils.FormataValor(reader[1]);

					return new
					{
						total_count = TotalCount,
						valor_total = ValorTotal,
						results = lstRetorno
					};
				}
			}
		}

		private dynamic LancamentosEstado(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();

				sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					AS (
						SELECT
						 d.id_estado
						, e.nome as nome_estado
						, count(l1.total_notas) as total_notas
						, sum(l1.valor_total) as valor_total
						from (

							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor_liquido) as valor_total
							, l.id_cf_deputado
							FROM cf_despesa l
							WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
						GROUP BY id_cf_deputado
					) l1
					INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado
					LEFT JOIN estado e on e.id = d.id_estado
					GROUP BY e.id, e.nome
				); "); //end table_in_memory

				sqlSelect.AppendLine(@"select * from table_in_memory ");
				AdicionaResultadoComum(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_estado = reader["id_estado"],
							nome_estado = reader["nome_estado"],
							total_notas = reader["total_notas"],
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();
					string ValorTotal = Utils.FormataValor(reader[1]);

					return new
					{
						total_count = TotalCount,
						valor_total = ValorTotal,
						results = lstRetorno
					};
				}
			}
		}

		private dynamic LancamentosNotaFiscal(FiltroParlamentarDTO filtro)
		{

			//sqlSelect.AppendLine(" p.IdeCadastro as IdCadastro");
			//sqlSelect.AppendLine(", p.nuDeputadoId as IdDeputado");
			//sqlSelect.AppendLine(", l.id as Id");
			//sqlSelect.AppendLine(", l.ideDocumento as IdDocumento");
			//sqlSelect.AppendLine(", l.txtNumero as NotaFiscal");
			//sqlSelect.AppendLine(", l.txtCNPJCPF AS Codigo");
			//sqlSelect.AppendLine(", l.numano as NumAno");
			//sqlSelect.AppendLine(", l.txtNumero as Numero");
			//sqlSelect.AppendLine(", l.datEmissao as DataEmissao");
			//sqlSelect.AppendLine(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
			//sqlSelect.AppendLine(", l.txNomeParlamentar as nome_parlamentar");
			//sqlSelect.AppendLine(", SUM(l.vlrLiquido) AS vlrTotal ");

			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();

				//sqlSelect.AppendLine("DROP TABLE IF EXISTS table_in_memory; ");
				//sqlSelect.AppendLine("CREATE TEMPORARY TABLE table_in_memory ");
				//sqlSelect.AppendLine("AS ( ");
				sqlSelect.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						 l.id as id_cf_despesa
						, l.data_emissao
						, l.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, d.id as id_deputado
						, d.nome_parlamentar
                        , l.numero_documento
						, l.valor_liquido
					FROM cf_despesa l
					INNER JOIN cf_deputado d on d.id = l.id_cf_deputado
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				sqlSelect.AppendFormat(" ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "l.data_emissao desc, l.valor_liquido desc" : Utils.MySqlEscape(filtro.sorting));
				sqlSelect.AppendFormat(" LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				sqlSelect.AppendLine("SELECT FOUND_ROWS();");

				sqlSelect.AppendLine(
					@"SELECT SUM(l.valor_liquido) as valor_total
					FROM cf_despesa l
					INNER JOIN cf_deputado d on d.id = l.id_cf_deputado
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
					WHERE (1=1)");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputado(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputado(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputado(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cf_despesa = reader["id_cf_despesa"],
							data_emissao = Utils.FormataData(reader["data_emissao"]),
							id_fornecedor = reader["id_fornecedor"],
							cnpj_cpf = reader["cnpj_cpf"],
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							id_deputado = reader["id_deputado"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
							numero_documento = reader["numero_documento"].ToString(),
							valor_liquido = Utils.FormataValor(reader["valor_liquido"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();

					reader.NextResult();
					reader.Read();
					string ValorTotal = Utils.FormataValor(reader[0]);

					return new
					{
						total_count = TotalCount,
						valor_total = ValorTotal,
						results = lstRetorno
					};
				}
			}
		}

		private static void AdicionaFiltroPeriodo(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			DateTime dataIni = DateTime.Today;
			DateTime dataFim = DateTime.Today;
			switch (filtro.Periodo)
			{
				case "1": //PERIODO_MES_ATUAL
					sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
					break;

				case "2": //PERIODO_MES_ANTERIOR
					dataIni = dataIni.AddMonths(-1);
					sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
					break;

				case "3": //PERIODO_MES_ULT_4
					dataIni = dataIni.AddMonths(-3);
					sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
					break;

				case "4": //PERIODO_ANO_ATUAL
					dataIni = new DateTime(dataIni.Year, 1, 1);
					sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
					break;

				case "5": //PERIODO_ANO_ANTERIOR
					dataIni = new DateTime(dataIni.Year, 1, 1).AddYears(-1);
					dataFim = new DateTime(dataIni.Year, 12, 31);
					sqlSelect.AppendFormat(" AND l.ano_mes BETWEEN {0} AND {1}", dataIni.ToString("yyyyMM"), dataFim.ToString("yyyyMM"));
					break;

				case "8": //PERIODO_MANDATO_55
					sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201502 AND 201901");
					break;

				case "7": //PERIODO_MANDATO_54
					sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201102 AND 201501");
					break;

				case "6": //PERIODO_MANDATO_53
					sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 200702 AND 201101");
					break;
			}
		}

		private static void AdicionaFiltroEstadoDeputado(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.Partido))
			{
				sqlSelect.AppendLine("	AND l.id_cf_deputado IN (SELECT id FROM cf_deputado where id_partido IN(" + Utils.MySqlEscapeNumberToIn(filtro.Partido) + ")) ");
			}
		}

		private static void AdicionaFiltroPartidoDeputado(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.Uf))
			{
				sqlSelect.AppendLine("	AND l.id_cf_deputado IN (SELECT id FROM cf_deputado where id_estado IN(" + Utils.MySqlEscapeNumberToIn(filtro.Uf) + ")) ");
			}
		}

		private static void AdicionaFiltroFornecedor(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.Fornecedor))
			{
				filtro.Fornecedor = String.Join("", System.Text.RegularExpressions.Regex.Split(filtro.Fornecedor, @"[^\d]"));

				if (!string.IsNullOrEmpty(filtro.Fornecedor))
				{
					if (filtro.Fornecedor.Length == 14 || filtro.Fornecedor.Length == 11)
					{
						using (Banco banco = new Banco())
						{
							var id_fornecedor =
								banco.ExecuteScalar("select id from fornecedor where cnpj_cpf = '" + Utils.RemoveCaracteresNaoNumericos(filtro.Fornecedor) + "'");

							if (!Convert.IsDBNull(id_fornecedor))
							{
								sqlSelect.AppendLine("	AND l.id_fornecedor =" + id_fornecedor + " ");
							}
						}
					}
					else
					{
						sqlSelect.AppendLine("	AND l.id_fornecedor =" + Utils.RemoveCaracteresNaoNumericos(filtro.Fornecedor) + " ");
					}
				}
			}
		}

		private static void AdicionaFiltroDespesa(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.Despesa))
			{
				sqlSelect.AppendLine("	AND l.id_cf_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(filtro.Despesa) + ") ");
			}
		}

		private static void AdicionaFiltroDeputado(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.IdParlamentar))
			{
				sqlSelect.AppendLine("	AND l.id_cf_deputado IN (" + Utils.MySqlEscapeNumberToIn(filtro.IdParlamentar) + ") ");
			}
		}

		private static void AdicionaResultadoComum(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			//sqlSelect.AppendLine("select * from table_in_memory ");
			sqlSelect.AppendFormat(" ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "valor_total desc" : Utils.MySqlEscape(filtro.sorting));
			sqlSelect.AppendFormat(" LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

			sqlSelect.AppendLine(
				@"SELECT count(1), sum(valor_total) as valor_total
				FROM table_in_memory; ");
		}

		public dynamic TipoDespesa()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine("SELECT id, descricao FROM cf_despesa_tipo ");
				strSql.AppendFormat("ORDER BY descricao ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader["id"].ToString(),
							text = reader["descricao"].ToString(),
						});
					}
				}
				return lstRetorno;
			}
		}

		public dynamic Secretarios(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						p.id as id_cf_deputado
						, p.nome_parlamentar
						, p.quantidade_secretarios
					from cf_deputado p
					where p.quantidade_secretarios > 0
				");

				if (!string.IsNullOrEmpty(filtro.NomeParlamentar))
				{
					strSql.AppendFormat("and p.nome_parlamentar LIKE '%{0}%' ", Utils.MySqlEscape(filtro.NomeParlamentar));
				}

				strSql.AppendFormat(" ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "p.quantidade_secretarios DESC, p.nome_parlamentar" : Utils.MySqlEscape(filtro.sorting));
				strSql.AppendFormat(" LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				strSql.AppendLine("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cf_deputado = reader["id_cf_deputado"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
							quantidade_secretarios = reader["quantidade_secretarios"].ToString()
						});
					}

					reader.NextResult();
					reader.Read();

					return new
					{
						total_count = reader[0],
						results = lstRetorno
					};
				}
			}
		}

		public dynamic SecretariosPorDeputado(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT
						s.id as id_cf_secretario
						, s.id_cf_gabinete
						, s.nome
						, s.orgao
						, s.data
					FROM cf_secretario s
					WHERE s.id_cf_deputado = @id
				");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					var lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cf_secretario = reader["id_cf_secretario"],
							id_cf_gabinete = reader["id_cf_gabinete"],
							nome = reader["nome"].ToString(),
							orgao = reader["orgao"].ToString(),
							data = Utils.FormataData(reader["data"])
						});
					}

					return lstRetorno;
				}
			}
		}

		public object ResumoPresenca(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT 
						year(s.data) as ano
						, sum(IF(sp.presente = 1, 1, 0)) as presenca
						, sum(IF(sp.presente = 0 and sp.justificativa = '', 1, 0)) as ausencia
						, sum(IF(sp.presente = 0 and sp.justificativa <> '', 1, 0)) as ausencia_justificada
					FROM cf_sessao_presenca sp
					inner join cf_sessao s on s.id = sp.id_cf_sessao
					where sp.id_cf_deputado = @id
					group by sp.id_cf_deputado, year(s.data)
					order by year(s.data)
				");
				banco.AddParameter("@id", id);

				var categories = new List<dynamic>();
				var series = new List<dynamic>();

				var presencas = new List<dynamic>();
				var ausencias = new List<dynamic>();
				var ausencias_justificadas = new List<dynamic>();

				int presenca_total = 0;
				int ausencia_total = 0;
				int ausencia_justificada_total = 0;

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							int presenca = Convert.ToInt32(reader["presenca"]);
							int ausencia = Convert.ToInt32(reader["ausencia"]);
							int ausencia_justificada = Convert.ToInt32(reader["ausencia_justificada"]);

							categories.Add(Convert.ToInt32(reader["ano"]));

							presencas.Add(presenca);
							ausencias.Add(ausencia);
							ausencias_justificadas.Add(ausencia_justificada);

							presenca_total += presenca;
							ausencia_total += ausencia;
							ausencia_justificada_total += ausencia_justificada;
						}
					}
					else
					{
						return new
						{
							frequencia_anual = new
							{
								categories = new List<dynamic>(),
								series = new List<dynamic>()
							},
							frequencia_total_percentual = new List<dynamic>()
						};
					}
				}

				series.Add(new
				{
					name = "Presença",
					stack = "presenca",
					data = presencas
				});

				series.Add(new
				{
					name = "Ausência justificada",
					stack = "ausencia",
					data = ausencias_justificadas
				});

				series.Add(new
				{
					name = "Ausência",
					stack = "ausencia",
					data = ausencias
				});

				int total_sessoes = presenca_total + ausencia_total + ausencia_justificada_total;

				var frequencia_total_percentual = new List<dynamic>
				{
					new {name = "Presença", y = presenca_total * 100 / total_sessoes},
					new {name = "Ausência Justificada", y = ausencia_justificada_total * 100 / total_sessoes},
					new {name = "Ausência", y = ausencia_total * 100 / total_sessoes}
				};

				return new
				{
					frequencia_anual = new
					{
						categories,
						series
					},
					frequencia_total_percentual
				};
			}

		}
	}
}