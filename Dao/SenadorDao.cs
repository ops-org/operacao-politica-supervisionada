using MySql.Data.MySqlClient;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OPS.Dao
{
	public class SenadorDao
	{
		private int IdDespesaOrdinal { get; set; }
		private int IdOrdinal { get; set; }
		private int IdCadastroOrdinal { get; set; }
		private int IdDocumentoOrdinal { get; set; }
		private int CodigoOrdinal { get; set; }
		private int AnoOrdinal { get; set; }
		private int NumeroOrdinal { get; set; }
		private int NomeDespesaOrdinal { get; set; }
		private int NomeParlamentarOrdinal { get; set; }
		private int NomeBeneficiarioOrdinal { get; set; }
		private int SiglaUfOrdinal { get; set; }
		private int SiglaPartidoOrdinal { get; set; }
		private int DataEmissaoOrdinal { get; set; }
		private int DataUltimaNotaFiscalOrdinal { get; set; }
		private int DoadorOrdinal { get; set; }
		private int TotalNotasOrdinal { get; set; }
		private int VlrTotalOrdinal { get; set; }

		internal dynamic Consultar(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT CodigoParlamentar, NomeParlamentar, SiglaUf, SiglaPartido, Url ");
				strSql.Append(", (SELECT SUM(p.Valor) FROM lancamentos_senadores p WHERE CodigoParlamentar = @id) as TotalGastoCEAPS ");
				strSql.Append("FROM senadores ");
				strSql.Append("WHERE CodigoParlamentar = @id ");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.Read())
					{
						return new
						{
							IdCadastro = reader["CodigoParlamentar"].ToString(),
							NomeParlamentar = reader["NomeParlamentar"].ToString(),
							SiglaUf = reader["SiglaUf"].ToString(),
							SiglaPartido = reader["SiglaPartido"].ToString(),
							Url = reader["Url"].ToString(),

							TotalGastoCEAPS = Utils.FormataValor(reader["TotalGastoCEAPS"]),
						};
					}

					return null;
				}
			}
		}

		internal dynamic MaioresFornecedores(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT");
				strSql.Append(" SUM(l.Valor) AS Valor");
				strSql.Append(", l.CNPJCPF AS CnpjCpf");
				strSql.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.Fornecedor), 1, 50) AS NomeBeneficiario");
				strSql.Append(" FROM lancamentos_senadores l ");
				strSql.Append(" LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.CNPJCPF");
				strSql.Append(" WHERE l.CodigoParlamentar = @id");
				strSql.Append(" GROUP BY CnpjCpf, NomeBeneficiario");
				strSql.Append(" ORDER BY Valor desc");
				strSql.Append(" LIMIT 10");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							CnpjCpf = reader["CnpjCpf"].ToString(),
							NomeBeneficiario = reader["NomeBeneficiario"].ToString(),
							VlrLiquido = Utils.FormataValor(reader["Valor"])
						});
					}

					return lstRetorno;
				}
			}
		}

		internal dynamic MaioresNotas(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT");
				strSql.Append(" l.id AS Id");
				strSql.Append(", l.Valor AS Valor");
				strSql.Append(", l.CNPJCPF AS CnpjCpf");
				strSql.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.Fornecedor), 1, 50) AS NomeBeneficiario");
				strSql.Append(" FROM lancamentos_senadores l ");
				strSql.Append(" LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.CNPJCPF");
				strSql.Append(" WHERE l.CodigoParlamentar = @id");
				strSql.Append(" ORDER BY Valor desc");
				strSql.Append(" LIMIT 10");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							Id = reader["Id"].ToString(),
							CnpjCpf = reader["CnpjCpf"].ToString(),
							NomeBeneficiario = reader["NomeBeneficiario"].ToString(),
							VlrLiquido = Utils.FormataValor(reader["Valor"])
						});
					}

					return lstRetorno;
				}
			}
		}

		internal dynamic Documento(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();

				strSql.Append("SELECT ");
				strSql.Append(" l.id as Id");
				strSql.Append(", l.NomeParlamentar as NomeParlamentar");
				strSql.Append(", p.SiglaUf");
				strSql.Append(", p.SiglaPartido");
				strSql.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.Fornecedor), 1, 50) AS NomeBeneficiario");
				strSql.Append(", l.CNPJCPF AS CnpjCpf");
				strSql.Append(", l.DataDoc as DataEmissao");
				strSql.Append(", l.Valor as Valor");
				strSql.Append(", l.Ano as Ano");
				strSql.Append(", l.Mes as Mes");
				strSql.Append(", p.CodigoParlamentar as IdCadastro");
				strSql.Append(" FROM lancamentos_senadores l");
				strSql.Append(" LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.CNPJCPF");
				strSql.Append(" LEFT JOIN senadores p ON p.CodigoParlamentar = l.CodigoParlamentar");
				strSql.Append(" LEFT JOIN despesas d ON d.numsubcota = l.CodigoDespesa");
				strSql.Append(" WHERE l.id = @id");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.Read())
					{
						var result = new
						{
							Id = reader["Id"],
							NomeParlamentar = reader["NomeParlamentar"].ToString(),
							Uf = reader["SiglaUf"].ToString(),
							Partido = reader["SiglaPartido"].ToString(),
							DataEmissao = Utils.FormataData(reader["DataEmissao"]),
							Valor = Utils.FormataValor(reader["Valor"]),
							Ano = reader["Ano"].ToString(),
							Mes = reader["Mes"].ToString(),
							CnpjCpf = reader["CnpjCpf"].ToString(),
							IdCadastro = reader["IdCadastro"],
						};

						return result;
					}

					return null;
				}
			}
		}

		internal dynamic GastosMensaisPorAno(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT l.Ano, l.Mes, SUM(l.Valor) AS VlrTotal ");
				strSql.Append("FROM lancamentos_senadores l ");
				strSql.Append("WHERE l.CodigoParlamentar = @id ");
				strSql.Append("group by l.Ano, l.Mes ");
				strSql.Append("order by l.Ano, l.Mes ");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					var lstValoresMensais = new decimal?[12];
					string anoControle = string.Empty;
					bool existeGastoNoAno = false;

					while (reader.Read())
					{
						if (reader[0].ToString() != anoControle)
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

							anoControle = reader[0].ToString();
						}

						if (Convert.ToDecimal(reader[2]) > 0)
						{
							lstValoresMensais[Convert.ToInt32(reader[1]) - 1] = Convert.ToDecimal(reader[2]);
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

		internal dynamic Pesquisa(FiltroDropDownDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS CodigoParlamentar, NomeParlamentar FROM senadores ");

				if (!string.IsNullOrEmpty(filtro.q))
				{
					strSql.AppendFormat("WHERE NomeParlamentar LIKE '%{0}%' ", filtro.q);
				}
				else if (!string.IsNullOrEmpty(filtro.qs))
				{
					strSql.AppendFormat("WHERE CodigoParlamentar IN({0}) ", filtro.qs);
				}

				strSql.AppendFormat("ORDER BY NomeParlamentar ");
				strSql.AppendFormat("LIMIT {0},{1}; ", ((filtro.page ?? 1) - 1) * filtro.count, filtro.count);

				strSql.Append("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader[0].ToString(),
							text = reader[1].ToString(),
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

		internal dynamic Lancamentos(FiltroParlamentarDTO filtro)
		{
			if (filtro == null) throw new ArgumentException("filtro");

			using (Banco banco = new Banco())
			{
				var sqlSelect = new StringBuilder();
				var sqlWhere = new StringBuilder();
				var sqlGroupBy = new StringBuilder();
				var sqlCount = new StringBuilder();

				sqlSelect.Append("SELECT SQL_CALC_FOUND_ROWS");
				sqlWhere.Append("WHERE (1=1)");
				sqlCount.AppendLine();

				switch (filtro.Agrupamento)
				{
					case eAgrupamentoAuditoria.Parlamentar:
						sqlSelect.Append(" l.CodigoParlamentar as IdCadastro");
						sqlSelect.Append(", p.NomeParlamentar");
						sqlSelect.Append(", p.SiglaUf");
						sqlSelect.Append(", p.SiglaPartido");
						sqlSelect.Append(", COUNT(id) AS TotalNotas");
						sqlSelect.Append(", SUM(l.Valor) AS VlrTotal ");
						sqlSelect.Append("FROM lancamentos_senadores l ");
						sqlSelect.Append("INNER JOIN senadores p on p.CodigoParlamentar = l.CodigoParlamentar ");

						sqlGroupBy.Append("GROUP BY l.CodigoParlamentar, p.NomeParlamentar, p.SiglaUf, p.SiglaPartido ");
						break;
					case eAgrupamentoAuditoria.Despesa:
						sqlSelect.Append(" l.CodigoDespesa as IdDespesa");
						sqlSelect.Append(", l.TipoDespesa");
						sqlSelect.Append(", SUM(l.Valor) AS VlrTotal ");
						sqlSelect.Append("FROM lancamentos_senadores l ");
						sqlSelect.Append("INNER JOIN senadores p on p.CodigoParlamentar = l.CodigoParlamentar ");

						sqlGroupBy.Append("GROUP BY l.TipoDespesa ");
						break;
					case eAgrupamentoAuditoria.Fornecedor:
						sqlSelect.Append(" l.CNPJCPF AS Codigo");
						sqlSelect.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.Fornecedor), 1, 50) AS NomeBeneficiario");
						sqlSelect.Append(", f.Uf as SiglaUf");
						sqlSelect.Append(", f.DataUltimaNotaFiscal");
						sqlSelect.Append(", CASE WHEN doador = 1 THEN 'Sim' ELSE '' END AS Doador");
						sqlSelect.Append(", SUM(l.Valor) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos_senadores l ");
						sqlSelect.Append("LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.CNPJCPF ");
						sqlSelect.Append("INNER JOIN senadores p on p.CodigoParlamentar = l.CodigoParlamentar ");

						sqlGroupBy.Append("GROUP BY l.CNPJCPF, IFNULL(f.txtbeneficiario, l.Fornecedor), f.Uf, f.DataUltimaNotaFiscal, f.doador ");
						break;
					case eAgrupamentoAuditoria.Partido:
						sqlSelect.Append(" p.SiglaPartido");
						sqlSelect.Append(", SUM(l.Valor) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos_senadores l ");
						sqlSelect.Append("INNER JOIN senadores p on p.CodigoParlamentar = l.CodigoParlamentar ");

						sqlGroupBy.Append("GROUP BY p.SiglaPartido ");
						break;
					case eAgrupamentoAuditoria.Uf:
						sqlSelect.Append(" p.SiglaUf");
						sqlSelect.Append(", SUM(l.Valor) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos_senadores l ");
						sqlSelect.Append("INNER JOIN senadores p on p.CodigoParlamentar = l.CodigoParlamentar ");

						sqlGroupBy.Append("GROUP BY p.SiglaUf ");
						break;
					case eAgrupamentoAuditoria.Documento:
						sqlSelect.Append(" p.CodigoParlamentar as IdCadastro");
						sqlSelect.Append(", l.id as Id");
						sqlSelect.Append(", l.Documento as IdDocumento");
						sqlSelect.Append(", l.CNPJCPF AS Codigo");
						sqlSelect.Append(", l.Ano as Ano");
						sqlSelect.Append(", l.DataDoc as DataEmissao");
						sqlSelect.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.Fornecedor), 1, 50) AS NomeBeneficiario");
						sqlSelect.Append(", p.NomeParlamentar");
						sqlSelect.Append(", SUM(l.Valor) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos_senadores l ");
						sqlSelect.Append("LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.CNPJCPF ");
						sqlSelect.Append("LEFT JOIN senadores p ON p.CodigoParlamentar = l.CodigoParlamentar ");

						sqlGroupBy.Append("GROUP BY IdCadastro, IdDocumento, Codigo, Ano, DataEmissao, NomeBeneficiario, NomeParlamentar ");
						break;
				}


				DateTime dataIni = DateTime.Today;
				DateTime dataFim = DateTime.Today;

				switch (filtro.Periodo)
				{
					case "1": //PERIODO_MES_ATUAL
						sqlWhere.Append(" AND l.anoMes = @anoMes");
						banco.AddParameter("anoMes", dataIni.ToString("yyyyMM"));
						break;

					case "2": //PERIODO_MES_ANTERIOR
						dataIni = dataIni.AddMonths(-1);
						dataFim = dataIni.AddMonths(-1);
						sqlWhere.Append(" AND l.anoMes = @anoMes");
						banco.AddParameter("anoMes", dataIni.ToString("yyyyMM"));
						break;

					case "3": //PERIODO_MES_ULT_4
						dataIni = dataIni.AddMonths(-3);
						sqlWhere.Append(" AND l.anoMes >= @anoMes");
						banco.AddParameter("anoMes", dataIni.ToString("yyyyMM"));
						break;

					case "4": //PERIODO_ANO_ATUAL
						dataIni = new DateTime(dataIni.Year, 1, 1);
						sqlWhere.Append(" AND l.anoMes >= @anoMes");
						banco.AddParameter("anoMes", dataIni.ToString("yyyyMM"));
						break;

					case "5": //PERIODO_ANO_ANTERIOR
						dataIni = new DateTime(dataIni.Year, 1, 1).AddYears(-1);
						dataFim = new DateTime(dataIni.Year, 12, 31);
						sqlWhere.Append(" AND l.anoMes BETWEEN @anoMesIni AND @anoMesFim");
						banco.AddParameter("anoMesIni", dataIni.ToString("yyyyMM"));
						banco.AddParameter("anoMesFim", dataFim.ToString("yyyyMM"));
						break;

					case "6": //PERIODO_MANDATO_55
						sqlWhere.Append(" AND l.anoMes BETWEEN 201502 AND 201901");
						break;

					case "7": //PERIODO_MANDATO_54
						sqlWhere.Append(" AND l.anoMes BETWEEN 201102 AND 201501");
						break;

					case "8": //PERIODO_MANDATO_53
						sqlWhere.Append(" AND l.anoMes BETWEEN 200702 AND 201101");
						break;

						//case PERIODO_INFORMAR:
						//	dataIni = new DateTime(Convert.ToInt32(anoIni), Convert.ToInt32(mesIni), 1);
						//	dataFim = new DateTime(Convert.ToInt32(anoFim), Convert.ToInt32(mesFim), 1);
						//	sqlWhere.Append(" AND l.anoMes BETWEEN @anoMesIni AND @anoMesFim");
						//	banco.AddParameter("anoMesIni", dataIni.ToString("yyyyMM"));
						//	banco.AddParameter("anoMesFim", dataFim.ToString("yyyyMM"));
						//	break;
				}

				if (!string.IsNullOrEmpty(filtro.IdParlamentar))
				{
					sqlWhere.Append(" AND l.CodigoParlamentar IN (" + filtro.IdParlamentar + ")");
				}

				if (!string.IsNullOrEmpty(filtro.Despesa))
				{
					sqlWhere.Append(" AND l.CodigoDespesa IN (" + filtro.Despesa + ")");
				}

				if (!string.IsNullOrEmpty(filtro.Fornecedor))
				{
					sqlWhere.Append(" AND l.CNPJCPF IN ('" + filtro.Fornecedor.Replace(",", "','").Replace(".", "").Replace("-", "").Replace("/", "").Replace("'", "") + "')");
				}

				if (!string.IsNullOrEmpty(filtro.Uf))
				{
					if (filtro.Agrupamento == eAgrupamentoAuditoria.Fornecedor)
					{
						sqlWhere.Append(" AND f.Uf IN ('" + filtro.Uf.Replace(",", "','") + "')");
					}
					else
					{
						sqlWhere.Append(" AND p.SiglaUf IN ('" + filtro.Uf.Replace(",", "','") + "')");
					}
				}

				if (!string.IsNullOrEmpty(filtro.Partido))
				{
					sqlWhere.Append(" AND p.SiglaPartido IN ('" + filtro.Partido.Replace(",", "','") + "')");
				}

				if (!string.IsNullOrEmpty(filtro.Documento))
				{
					sqlWhere.Append(" AND l.id  = " + filtro.Documento);
				}

				sqlGroupBy.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
				sqlGroupBy.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				sqlCount.Append("SELECT ");
				sqlCount.Append("SUM(l.Valor) AS VlrTotal ");
				sqlCount.Append("FROM lancamentos_senadores l ");
				sqlCount.Append("INNER JOIN fornecedores f ON f.txtCNPJCPF = l.CNPJCPF ");
				sqlCount.Append("INNER JOIN senadores p ON p.CodigoParlamentar = l.CodigoParlamentar ");
				

				sqlSelect.AppendLine();
				sqlWhere.AppendLine();
				sqlGroupBy.AppendLine();
				sqlCount.AppendLine();

				string sqlCompleto =
					sqlSelect.ToString() + sqlWhere.ToString() + sqlGroupBy.ToString() +
					"SELECT FOUND_ROWS();" +
					sqlCount.ToString() + sqlWhere.ToString();

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(sqlCompleto))
				{
					switch (filtro.Agrupamento)
					{
						case eAgrupamentoAuditoria.Parlamentar:
							IdCadastroOrdinal = reader.GetOrdinal("IdCadastro");
							NomeParlamentarOrdinal = reader.GetOrdinal("NomeParlamentar");
							SiglaUfOrdinal = reader.GetOrdinal("SiglaUf");
							SiglaPartidoOrdinal = reader.GetOrdinal("SiglaPartido");
							TotalNotasOrdinal = reader.GetOrdinal("TotalNotas");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									IdCadastro = reader[IdCadastroOrdinal],
									NomeParlamentar = reader[NomeParlamentarOrdinal],
									SiglaUf = reader[SiglaUfOrdinal],
									SiglaPartido = reader[SiglaPartidoOrdinal],
									TotalNotas = reader[TotalNotasOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Despesa:
							IdDespesaOrdinal = reader.GetOrdinal("IdDespesa");
							NomeDespesaOrdinal = reader.GetOrdinal("TipoDespesa");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									IdDespesa = reader[IdDespesaOrdinal],
									NomeDespesa = reader[NomeDespesaOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Fornecedor:
							CodigoOrdinal = reader.GetOrdinal("Codigo");
							NomeBeneficiarioOrdinal = reader.GetOrdinal("NomeBeneficiario");
							SiglaUfOrdinal = reader.GetOrdinal("SiglaUf");
							DataUltimaNotaFiscalOrdinal = reader.GetOrdinal("DataUltimaNotaFiscal");
							DoadorOrdinal = reader.GetOrdinal("Doador");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									Codigo = reader[CodigoOrdinal],
									NomeBeneficiario = reader[NomeBeneficiarioOrdinal],
									SiglaUf = reader[SiglaUfOrdinal],
									DataUltimaNotaFiscal = Utils.FormataData(reader[DataUltimaNotaFiscalOrdinal]),
									Doador = reader[DoadorOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}

							sqlSelect.Append(", f.DataUltimaNotaFiscal");
							sqlSelect.Append(", CASE WHEN doador = 1 THEN 'Sim' ELSE '' END AS Doador");
							break;
						case eAgrupamentoAuditoria.Partido:
							SiglaPartidoOrdinal = reader.GetOrdinal("SiglaPartido");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									SiglaPartido = reader[SiglaPartidoOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Uf:
							SiglaUfOrdinal = reader.GetOrdinal("SiglaUf");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									SiglaUf = reader[SiglaUfOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Documento:
							IdOrdinal = reader.GetOrdinal("Id");
							IdCadastroOrdinal = reader.GetOrdinal("IdCadastro");
							IdDocumentoOrdinal = reader.GetOrdinal("IdDocumento");
							CodigoOrdinal = reader.GetOrdinal("Codigo");
							AnoOrdinal = reader.GetOrdinal("Ano");
							DataEmissaoOrdinal = reader.GetOrdinal("DataEmissao");
							NomeBeneficiarioOrdinal = reader.GetOrdinal("NomeBeneficiario");
							NomeParlamentarOrdinal = reader.GetOrdinal("NomeParlamentar");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									Id = reader[IdOrdinal],
									IdCadastro = reader[IdCadastroOrdinal],
									IdDocumento = reader[IdDocumentoOrdinal],
									Codigo = reader[CodigoOrdinal],
									Ano = reader[AnoOrdinal],
									DataEmissao = Utils.FormataData(reader[DataEmissaoOrdinal]),
									NomeBeneficiario = reader[NomeBeneficiarioOrdinal],
									NomeParlamentar = reader[NomeParlamentarOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							// SUBSTRING(IFNULL(f.txtbeneficiario, l.Fornecedor), 1, 50) AS NomeBeneficiario");
							// l.NomeParlamentar as NomeParlamentar");
							// SUM(l.Valor) AS vlrTotal ");
							break;
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

		internal dynamic TipoDespesa(FiltroDropDownDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS CodigoDespesa, TipoDespesa FROM despesas_senadores ");
				if (!string.IsNullOrEmpty(filtro.q))
				{
					strSql.AppendFormat("WHERE TipoDespesa LIKE @q ", filtro.q);
					banco.AddParameter("@q", "%" + filtro.q + "%");
				}
				strSql.AppendFormat("ORDER BY TipoDespesa ");
				strSql.AppendFormat("LIMIT {0},{1}; ", ((filtro.page ?? 1) - 1) * filtro.count, filtro.count);

				strSql.Append("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							id = reader[0].ToString(),
							text = reader[1].ToString(),
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

		internal dynamic Secretarios(FiltroParlamentarDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
				strSql.Append("p.CodigoParlamentar as IdCadastro, p.NomeParlamentar as NomeParlamentar, count(s.deputado) as Quantidade ");
				strSql.Append("FROM secretario s ");
				strSql.Append("INNER JOIN senadores p ON p.CodigoParlamentar = s.deputado ");

				if (!string.IsNullOrEmpty(filtro.NomeParlamentar))
				{
					strSql.AppendFormat("WHERE NomeParlamentar LIKE '%{0}%' ", filtro.NomeParlamentar);
				}

				strSql.AppendFormat("GROUP BY p.CodigoParlamentar, p.NomeParlamentar ");

				strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "Quantidade DESC, p.NomeParlamentar" : filtro.sorting);
				strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				strSql.Append("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							IdCadastro = reader[0].ToString(),
							NomeParlamentar = reader[1].ToString(),
							Quantidade = reader[2].ToString()
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

		internal dynamic SecretariosPorDeputado(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT ");
				strSql.Append("s.nome, p.NomeParlamentar as nomeParlamentar ");
				strSql.Append("FROM secretario s ");
				strSql.Append("INNER JOIN senadores p on p.CodigoParlamentar = s.deputado ");
				strSql.Append("WHERE p.CodigoParlamentar = @id ");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					var lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							NomeSecretario = reader[0].ToString(),
							NomeParlamentar = reader[1].ToString()
						});
					}

					return lstRetorno;
				}
			}
		}
	}
}