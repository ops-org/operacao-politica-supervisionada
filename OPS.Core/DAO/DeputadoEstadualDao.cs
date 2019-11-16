using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using OPS.Core.DTO;

namespace OPS.Core.DAO
{
	public class DeputadoEstadualDao
	{
		public dynamic Consultar(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = @"
					SELECT 
						d.id as id_cl_deputado
						, d.nome_parlamentar
						, d.nome_civil
						, d.nascimento
						, d.sexo
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
						, d.email
						, d.valor_total
					FROM cl_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					WHERE d.id = @id
				";
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql))
				{
					if (reader.Read())
					{
						return new
						{
							id_cl_deputado = reader["id_cl_deputado"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            nascimento = Utils.FormataData(reader["nascimento"]),
                            sexo = reader["sexo"].ToString(),
                            id_partido = reader["id_partido"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_partido = reader["nome_partido"].ToString(),
                            id_estado = reader["id_estado"],
                            sigla_partido = reader["sigla_partido"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            email = reader["email"].ToString(),

                            valor_total = Utils.FormataValor(reader["valor_total"]),
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
						, pj.nome  AS nome_fornecedor
						, l1.valor_total
					from (
						SELECT
							l.id_fornecedor
							, SUM(l.valor) as valor_total
						FROM cl_despesa l
						WHERE l.id_cl_deputado = @id
						GROUP BY l.id_fornecedor
						order by valor_total desc
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
						 l1.id as id_cl_despesa
						, l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome  AS nome_fornecedor
						, l1.valor
					from (
						SELECT
						l.id
						, l.valor
						, l.id_fornecedor
						FROM cl_despesa l
						WHERE l.id_cl_deputado = @id
						order by l.valor desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor desc 
				");

				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cl_despesa = reader["id_cl_despesa"].ToString(),
							id_fornecedor = reader["id_fornecedor"].ToString(),
							cnpj_cpf = reader["cnpj_cpf"].ToString(),
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							valor = Utils.FormataValor(reader["valor"])
						});
					}

					return lstRetorno;
				}
			}
		}

		public dynamic GastosMensaisPorAno(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT YEAR(d.data) as ano, MONTH(d.data) as mes, SUM(d.valor) AS valor_total
					FROM cl_despesa d
					WHERE d.id_cl_deputado = @id
					group by YEAR(DATA), MONTH(data)
					order BY ano, mes
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

        public dynamic Busca(string value)
        {
            using (Banco banco = new Banco())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						s.id as id_cl_deputado
						, s.nome_parlamentar
						, s.nome_civil
						, s.valor_total
						, s.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, s.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
					FROM cl_deputado s
					LEFT JOIN partido p on p.id = s.id_partido
					LEFT JOIN estado e on e.id = s.id_estado
                    WHERE valor_total > 0");

                if (!string.IsNullOrEmpty(value))
                {
                    strSql.AppendLine("	AND (s.nome_parlamentar like '%" + Utils.MySqlEscape(value) + "%' or s.nome_civil like '%" + Utils.MySqlEscape(value) + "%')");
                }

                strSql.AppendLine(@"
                    ORDER BY nome_parlamentar
                    limit 100
				");

                var lstRetorno = new List<dynamic>();
                using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
                {
                    while (reader.Read())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_deputado = reader["id_cl_deputado"],
                            nome = reader["nome_parlamentar"].ToString(),
                            nome_completo = reader["nome_civil"].ToString(),
                            valor_total = Utils.FormataValor(reader["valor_total"]),
                            id_partido = reader["id_partido"],
                            sigla_partido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            nome_partido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "SEM PARTIDO",
                            id_estado = reader["id_estado"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_estado = reader["nome_estado"].ToString()
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public dynamic Pesquisa()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT 
						id, nome_parlamentar, nome_civil
					FROM cl_deputado
					WHERE id IN (
						/* Somente deputados com despesas */
						select distinct id_cl_deputado 
						from cl_despesa
					)
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
							tokens = new[] { reader["nome_parlamentar"].ToString(), reader["nome_civil"].ToString() },
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
				case EnumAgrupamentoAuditoria.Parlamentar:
					return LancamentosParlamentar(filtro);
				case EnumAgrupamentoAuditoria.Despesa:
					return LancamentosDespesa(filtro);
				case EnumAgrupamentoAuditoria.Fornecedor:
					return LancamentosFornecedor(filtro);
				case EnumAgrupamentoAuditoria.Partido:
					return LancamentosPartido(filtro);
				case EnumAgrupamentoAuditoria.Uf:
					return LancamentosEstado(filtro);
				case EnumAgrupamentoAuditoria.Documento:
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
					, sum(l.valor) as valor_total
					, l.id_cl_deputado
					FROM cl_despesa l
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputadoEstadual(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
					GROUP BY id_cl_deputado;
					
					SELECT
						 d.id as id_cl_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, l1.total_notas
						, l1.valor_total
						from table_in_memory l1
					LEFT JOIN cl_deputado d on d.id = l1.id_cl_deputado
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
							id_cl_deputado = reader["id_cl_deputado"],
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
						, sum(l.valor) as valor_total
					FROM cl_despesa l
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputadoEstadual(filtro, sqlSelect);

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
						, sum(l.valor) as valor_total
						, l.id_cl_despesa_tipo
					FROM cl_despesa l
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputadoEstadual(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
					GROUP BY id_cl_despesa_tipo;
					
					SELECT
						l1.id_cl_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					from table_in_memory l1
					LEFT JOIN cl_despesa_tipo td on td.id = l1.id_cl_despesa_tipo
				");

				AdicionaResultadoComum(filtro, sqlSelect);

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cl_despesa_tipo = reader["id_cl_despesa_tipo"],
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
						, sum(l1.total_notas) as total_notas
                        , count(l1.id_cl_deputado) as total_deputado
                        , sum(l1.valor_total) / count(l1.id_cl_deputado) as valor_medio_por_deputado
						, sum(l1.valor_total) as valor_total
						from (
							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor) as valor_total
							, l.id_cl_deputado
							FROM cl_despesa l
							WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputadoEstadual(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
						GROUP BY id_cl_deputado
					) l1
					INNER JOIN cl_deputado d on d.id = l1.id_cl_deputado
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
                            total_deputado = reader["total_deputado"],
                            valor_medio_por_deputado = Utils.FormataValor(reader["valor_medio_por_deputado"]),
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
							, sum(l.valor) as valor_total
							, l.id_cl_deputado
							FROM cl_despesa l
							WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputadoEstadual(filtro, sqlSelect);

				sqlSelect.AppendLine(@"
						GROUP BY id_cl_deputado
					) l1
					INNER JOIN cl_deputado d on d.id = l1.id_cl_deputado
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
						 l.id as id_cl_despesa
						, l.data
						, l.id_fornecedor
						, pj.cnpj_cpf
						, IFNULL(l.observacao, pj.nome) AS nome_fornecedor
						, d.id as id_deputado
						, d.nome_parlamentar
                        , dt.descricao as despesa_tipo
                        , l.numero_documento
						, l.valor as valor_total
					FROM cl_despesa l
					INNER JOIN cl_deputado d on d.id = l.id_cl_deputado
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
					LEFT JOIN cl_despesa_tipo dt on dt.id = l.id_cl_despesa_tipo
					WHERE (1=1)
				");

				AdicionaFiltroPeriodo(filtro, sqlSelect);

				AdicionaFiltroDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroDespesa(filtro, sqlSelect);

				AdicionaFiltroFornecedor(filtro, sqlSelect);

				AdicionaFiltroPartidoDeputadoEstadual(filtro, sqlSelect);

				AdicionaFiltroEstadoDeputadoEstadual(filtro, sqlSelect);

				sqlSelect.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "l.data desc, l.valor desc" : Utils.MySqlEscape(filtro.sorting));
				sqlSelect.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				sqlSelect.AppendFormat("SELECT FOUND_ROWS();");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlSelect.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id_cl_despesa = reader["id_cl_despesa"],
							data = Utils.FormataData(reader["data"]),
							id_fornecedor = reader["id_fornecedor"],
							cnpj_cpf = reader["cnpj_cpf"],
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							id_deputado = reader["id_deputado"],
							nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            despesa_tipo = reader["despesa_tipo"].ToString(),
                            numero_documento = reader["numero_documento"].ToString(),
							valor_total = Utils.FormataValor(reader["valor_total"])
						});
					}

					reader.NextResult();
					reader.Read();
					string TotalCount = reader[0].ToString();
					string ValorTotal = null; //Utils.FormataValor(reader[1]);

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
					sqlSelect.AppendLine(string.Format(" AND l.ano_mes >= {0}01", dataIni.Year));
					break;

				case "5": //PERIODO_ANO_ANTERIOR
					sqlSelect.AppendLine(string.Format(" AND l.ano_mes BETWEEN {0}01 AND {0}12", dataIni.Year - 1));
					break;

                case "9": //PERIODO_MANDATO_56
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201902 AND 202301");
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

		private static void AdicionaFiltroEstadoDeputadoEstadual(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.Partido))
			{
				sqlSelect.AppendLine("	AND l.id_cl_deputado IN (SELECT id FROM cl_deputado where id_partido IN(" + Utils.MySqlEscapeNumberToIn(filtro.Partido) + ")) ");
			}
		}

		private static void AdicionaFiltroPartidoDeputadoEstadual(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.Uf))
			{
				sqlSelect.AppendLine("	AND l.id_cl_deputado IN (SELECT id FROM cl_deputado where id_estado IN(" + Utils.MySqlEscapeNumberToIn(filtro.Uf) + ")) ");
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
				sqlSelect.AppendLine("	AND l.id_cl_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(filtro.Despesa) + ") ");
			}
		}

		private static void AdicionaFiltroDeputadoEstadual(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			if (!string.IsNullOrEmpty(filtro.IdParlamentar))
			{
				sqlSelect.AppendLine("	AND l.id_cl_deputado IN (" + Utils.MySqlEscapeNumberToIn(filtro.IdParlamentar) + ") ");
			}
		}

		private static void AdicionaResultadoComum(FiltroParlamentarDTO filtro, StringBuilder sqlSelect)
		{
			//sqlSelect.AppendLine("select * from table_in_memory ");
			sqlSelect.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "valor_total desc" : Utils.MySqlEscape(filtro.sorting));
			sqlSelect.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

			sqlSelect.AppendLine(
				@"SELECT count(1), sum(valor_total) as valor_total
				FROM table_in_memory; ");
		}

		public dynamic TipoDespesa()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine("SELECT id, descricao FROM cl_despesa_tipo ");
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

		public dynamic SenadoResumoMensal()
		{
			using (Banco banco = new Banco())
			{
				using (MySqlDataReader reader = banco.ExecuteReader(@"select ano, mes, valor from cl_despesa_resumo_mensal"))
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

						if (Convert.ToDecimal(reader["valor"]) > 0)
						{
							lstValoresMensais[Convert.ToInt32(reader["mes"]) - 1] = Convert.ToDecimal(reader["valor"]);
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
				}
			}
		}

		public dynamic SenadoResumoAnual()
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					select ano, sum(valor) as valor
					from cl_despesa_resumo_mensal
					group by ano
				");

				var categories = new List<dynamic>();
				var series = new List<dynamic>();

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.HasRows)
					{
						while (reader.Read())
						{

							categories.Add(Convert.ToInt32(reader["ano"]));
							series.Add(Convert.ToDecimal(reader["valor"]));
						}
					}
				}

				return new
				{
					categories,
					series
				};
			}
		}
	}
}