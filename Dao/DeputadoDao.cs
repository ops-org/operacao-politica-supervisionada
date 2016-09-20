using MySql.Data.MySqlClient;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OPS.Dao
{
	public class DeputadoDao
	{
		private int IdDespesaOrdinal { get; set; }
		private int IdOrdinal { get; set; }
		private int IdCadastroOrdinal { get; set; }
		private int IdDeputadoOrdinal { get; set; }
		private int IdDocumentoOrdinal { get; set; }
		private int CodigoOrdinal { get; set; }
		private int NumAnoOrdinal { get; set; }
		private int NumeroOrdinal { get; set; }
		private int NomeDespesaOrdinal { get; set; }
		private int NomeParlamentarOrdinal { get; set; }
		private int NomeBeneficiarioOrdinal { get; set; }
		private int SgUfOrdinal { get; set; }
		private int SgPartidoOrdinal { get; set; }
		private int DataEmissaoOrdinal { get; set; }
		private int DataUltimaNotaFiscalOrdinal { get; set; }
		private int DoadorOrdinal { get; set; }
		private int AuditeiOrdinal { get; set; }
		private int TotalNotasOrdinal { get; set; }
		private int VlrTotalOrdinal { get; set; }

		internal dynamic Consultar(int id)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT ideCadastro, txNomeParlamentar, uf, partido, condicao, email, nome ");
				strSql.Append(", (SELECT SUM(p.VlrLiquido) FROM lancamentos p WHERE ideCadastro = @id) as TotalGastoCEAP ");
				strSql.Append(", (SELECT COUNT(1) FROM secretario WHERE deputado = @id) as TotalSecretarios ");
				strSql.Append("FROM parlamentares ");
				strSql.Append("WHERE ideCadastro = @id ");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.Read())
					{
						return new
						{
							IdCadastro = reader["ideCadastro"].ToString(),
							NomeParlamentar = reader["txNomeParlamentar"].ToString(),
							SgUf = reader["uf"].ToString(),
							SgPartido = reader["partido"].ToString(),
							Condicao = reader["condicao"].ToString(),
							Email = reader["email"].ToString(),
							Nome = reader["nome"].ToString(),

							TotalGastoCEAP = Utils.FormataValor(reader["TotalGastoCEAP"]),
							TotalSecretarios = reader["TotalSecretarios"].ToString(),
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
				strSql.Append(" SUM(l.VlrLiquido) AS VlrLiquido");
				strSql.Append(", l.txtCNPJCPF AS CnpjCpf");
				strSql.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
				strSql.Append(" FROM lancamentos l ");
				strSql.Append(" LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.txtCNPJCPF");
				strSql.Append(" WHERE l.IdeCadastro = @id");
				strSql.Append(" GROUP BY CnpjCpf, NomeBeneficiario");
				strSql.Append(" ORDER BY VlrLiquido desc");
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
							VlrLiquido = Utils.FormataValor(reader["VlrLiquido"])
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
				strSql.Append(", l.VlrLiquido AS VlrLiquido");
				strSql.Append(", l.txtCNPJCPF AS CnpjCpf");
				strSql.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
				strSql.Append(" FROM lancamentos l ");
				strSql.Append(" LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.txtCNPJCPF");
				strSql.Append(" WHERE l.IdeCadastro = @id");
				strSql.Append(" ORDER BY VlrLiquido desc");
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
							VlrLiquido = Utils.FormataValor(reader["VlrLiquido"])
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
				strSql.Append(", l.txNomeParlamentar as NomeParlamentar");
				strSql.Append(", l.nuCarteiraParlamentar as CarteiraParlamentar");
				strSql.Append(", l.nuLegislatura as Legislatura");
				strSql.Append(", l.sgUF as Uf");
				strSql.Append(", l.sgPartido as Partido");
				strSql.Append(", l.codLegislatura as CdLegislatura");
				strSql.Append(", d.txtDescricao as SubCota");
				strSql.Append(", l.txtDescricao as Descricao");
				//strSql.Append(", l.numEspecificacaoSubCota as EspecificacaoSubCota");
				strSql.Append(", l.txtDescricaoEspecificacao as DescricaoEspecificacao");
				strSql.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
				strSql.Append(", l.txtCNPJCPF AS CnpjCpf");
				strSql.Append(", l.txtNumero as Numero");
				strSql.Append(", l.indTipoDocumento as TipoDocumento");
				strSql.Append(", l.datEmissao as DataEmissao");
				strSql.Append(", l.vlrDocumento as VlrDocumento");
				strSql.Append(", l.vlrGlosa as VlrGlosa");
				strSql.Append(", l.vlrLiquido as VlrLiquido");
				strSql.Append(", l.numParcela as Parcela");
				strSql.Append(", l.txtPassageiro as NomePassageiro");
				strSql.Append(", l.txtTrecho as TrechoViagem");
				strSql.Append(", l.numLote as Lote");
				//strSql.Append(", l.numRessarcimento as Ressarcimento");
				strSql.Append(", l.ideDocumento as IdDocumento");
				strSql.Append(", l.vlrRestituicao as VlrRestituicao");
				strSql.Append(", l.numAno as Ano");
				strSql.Append(", l.numMes as Mes");
				strSql.Append(", p.ideCadastro as IdCadastro");
				strSql.Append(", p.nuDeputadoId as IdDeputado");
				strSql.Append(" FROM lancamentos l");
				strSql.Append(" LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.txtCNPJCPF");
				strSql.Append(" LEFT JOIN parlamentares p ON p.ideCadastro = l.ideCadastro");
				strSql.Append(" LEFT JOIN despesas d ON d.numsubcota = l.numSubCota");
				strSql.Append(" WHERE l.id = @id");
				banco.AddParameter("@id", id);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					if (reader.Read())
					{
						string sTipoDocumento = "";
						switch (reader["TipoDocumento"].ToString())
						{
							case "0": sTipoDocumento = "Nota Fiscal"; break;
							case "1": sTipoDocumento = "Recibo"; break;
							case "2": sTipoDocumento = "Despesa no Exterior"; break;
						}
						var result = new
						{
							Id = reader["Id"],
							NomeParlamentar = reader["NomeParlamentar"].ToString(),
							CarteiraParlamentar = reader["CarteiraParlamentar"].ToString(),
							Legislatura = reader["Legislatura"].ToString(),
							Uf = reader["Uf"].ToString(),
							Partido = reader["Partido"].ToString(),
							CdLegislatura = reader["CdLegislatura"],
							SubCota = reader["SubCota"].ToString(),
							Descricao = reader["Descricao"].ToString(),
							//EspecificacaoSubCota = reader["EspecificacaoSubCota"].ToString(),
							DescricaoEspecificacao = reader["DescricaoEspecificacao"].ToString(),
							NomeBeneficiario = reader["NomeBeneficiario"].ToString(),
							//Codigo = reader["Codigo"].ToString(),
							Numero = reader["Numero"].ToString(),
							TipoDocumento = sTipoDocumento,
							DataEmissao = Utils.FormataData(reader["DataEmissao"]),
							VlrDocumento = Utils.FormataValor(reader["VlrDocumento"]),
							VlrGlosa = Utils.FormataValor(reader["VlrGlosa"]),
							VlrLiquido = Utils.FormataValor(reader["VlrLiquido"]),
							Parcela = reader["Parcela"].ToString(),
							NomePassageiro = reader["NomePassageiro"].ToString(),
							TrechoViagem = reader["TrechoViagem"].ToString(),
							//Ressarcimento = reader["Ressarcimento"].ToString(),
							IdDocumento = reader["IdDocumento"],
							VlrRestituicao = Utils.FormataValor(reader["VlrRestituicao"]),
							Ano = reader["Ano"].ToString(),
							Mes = reader["Mes"].ToString(),
							CnpjCpf = reader["CnpjCpf"].ToString(),
							IdCadastro = reader["IdCadastro"],
							IdDeputado = reader["IdDeputado"],
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
				strSql.Append("SELECT l.numAno, l.numMes, SUM(l.VlrLiquido) AS VlrTotal ");
				strSql.Append("FROM lancamentos l ");
				strSql.Append("WHERE l.IdeCadastro = @id ");
				strSql.Append("group by l.numAno, l.numMes ");
				strSql.Append("order by l.numAno, l.numMes ");
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
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS ideCadastro, txNomeParlamentar FROM parlamentares ");

				if (!string.IsNullOrEmpty(filtro.q))
				{
					strSql.AppendFormat("WHERE txNomeParlamentar LIKE '%{0}%' ", filtro.q);
				}
				else if (!string.IsNullOrEmpty(filtro.qs))
				{
					strSql.AppendFormat("WHERE ideCadastro IN({0}) ", filtro.qs);
				}

				strSql.AppendFormat("ORDER BY txNomeParlamentar ");
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
						sqlSelect.Append(" l.IdeCadastro as IdCadastro");
						sqlSelect.Append(", l.txNomeParlamentar as NomeParlamentar");
						sqlSelect.Append(", p.uf as sgUF");
						sqlSelect.Append(", p.partido as SgPartido");
						sqlSelect.Append(", COUNT(id) AS TotalNotas");
						sqlSelect.Append(", SUM(l.VlrLiquido) AS VlrTotal ");
						sqlSelect.Append("FROM lancamentos l ");
						sqlSelect.Append("INNER JOIN parlamentares p on p.ideCadastro = l.ideCadastro ");

						sqlGroupBy.Append("GROUP BY l.IdeCadastro, l.txNomeParlamentar, p.uf, p.partido ");
						break;
					case eAgrupamentoAuditoria.Despesa:
						sqlSelect.Append(" l.numSubCota as IdDespesa");
						sqlSelect.Append(", l.txtDescricao as NomeDespesa");
						sqlSelect.Append(", SUM(l.VlrLiquido) AS VlrTotal ");
						sqlSelect.Append("FROM lancamentos l ");

						sqlGroupBy.Append("GROUP BY l.txtDescricao ");
						break;
					case eAgrupamentoAuditoria.Fornecedor:
						sqlSelect.Append(" l.txtCNPJCPF AS Codigo");
						sqlSelect.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
						sqlSelect.Append(", f.uf as SgUf");
						sqlSelect.Append(", f.DataUltimaNotaFiscal");
						sqlSelect.Append(", CASE WHEN UserName IS NULL THEN '' ELSE 'Sim' END AS Auditei");
						sqlSelect.Append(", CASE WHEN doador = 1 THEN 'Sim' ELSE '' END AS Doador");
						sqlSelect.Append(", SUM(l.vlrLiquido) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos l ");
						sqlSelect.Append("LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.txtCNPJCPF ");
						sqlSelect.Append("LEFT JOIN fornecedores_visitado fv ON fv.txtCNPJCPF = l.txtCNPJCPF ");

						sqlSelect.Append("AND fv.UserName = @UserName ");
						banco.AddParameter("UserName", System.Web.HttpContext.Current.User.Identity.Name);

						sqlGroupBy.Append("GROUP BY l.txtCNPJCPF, IFNULL(f.txtbeneficiario, l.txtbeneficiario), f.uf, f.DataUltimaNotaFiscal, fv.UserName, f.doador ");


						break;
					case eAgrupamentoAuditoria.Partido:
						sqlSelect.Append(" l.sgPartido as SgPartido");
						sqlSelect.Append(", SUM(l.vlrLiquido) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos l ");

						sqlGroupBy.Append("GROUP BY sgPartido ");
						break;
					case eAgrupamentoAuditoria.Uf:
						sqlSelect.Append(" l.sgUF as SgUf");
						sqlSelect.Append(", SUM(l.vlrLiquido) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos l ");

						sqlGroupBy.Append("GROUP BY sgUF ");
						break;
					case eAgrupamentoAuditoria.Documento:
						sqlSelect.Append(" p.ideCadastro as IdCadastro");
						sqlSelect.Append(", p.nuDeputadoId as IdDeputado");
						sqlSelect.Append(", l.id as Id");
						sqlSelect.Append(", l.ideDocumento as IdDocumento");
						sqlSelect.Append(", l.txtNumero as NotaFiscal");
						sqlSelect.Append(", l.txtCNPJCPF AS Codigo");
						sqlSelect.Append(", l.numano as NumAno");
						sqlSelect.Append(", l.txtNumero as Numero");
						sqlSelect.Append(", l.datEmissao as DataEmissao");
						sqlSelect.Append(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
						sqlSelect.Append(", l.txNomeParlamentar as NomeParlamentar");
						sqlSelect.Append(", SUM(l.vlrLiquido) AS vlrTotal ");
						sqlSelect.Append("FROM lancamentos l ");
						sqlSelect.Append("LEFT JOIN fornecedores f ON f.txtCNPJCPF = l.txtCNPJCPF ");
						sqlSelect.Append("LEFT JOIN parlamentares p ON p.ideCadastro = l.ideCadastro ");

						sqlGroupBy.Append("GROUP BY IdCadastro, IdDeputado, IdDocumento, Codigo, NumAno, Numero, DataEmissao, NomeBeneficiario, NomeParlamentar ");
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
					sqlWhere.Append(" AND l.ideCadastro IN (" + filtro.IdParlamentar + ")");
				}

				if (!string.IsNullOrEmpty(filtro.Despesa))
				{
					sqlWhere.Append(" AND l.numSubCota IN (" + filtro.Despesa + ")");
				}

				if (!string.IsNullOrEmpty(filtro.Fornecedor))
				{
					sqlWhere.Append(" AND l.txtCNPJCPF IN ('" + filtro.Fornecedor.Replace(",", "','").Replace(".", "").Replace("-", "").Replace("/", "").Replace("'", "") + "')");
				}

				if (!string.IsNullOrEmpty(filtro.Uf))
				{
					sqlWhere.Append(" AND l.sgUF IN ('" + filtro.Uf.Replace(",", "','") + "')");
				}


				if (!string.IsNullOrEmpty(filtro.Partido))
				{
					sqlWhere.Append(" AND l.sgPartido IN ('" + filtro.Partido.Replace(",", "','") + "')");
				}

				if (!string.IsNullOrEmpty(filtro.Documento))
				{
					sqlWhere.Append(" AND l.id  = " + filtro.Documento);
				}

				sqlGroupBy.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
				sqlGroupBy.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				sqlCount.Append("SELECT ");
				sqlCount.Append("SUM(l.VlrLiquido) AS VlrTotal ");
				sqlCount.Append("FROM lancamentos l");

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
							SgUfOrdinal = reader.GetOrdinal("SgUF");
							SgPartidoOrdinal = reader.GetOrdinal("SgPartido");
							TotalNotasOrdinal = reader.GetOrdinal("TotalNotas");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									IdCadastro = reader[IdCadastroOrdinal],
									NomeParlamentar = reader[NomeParlamentarOrdinal],
									SgUf = reader[SgUfOrdinal],
									SgPartido = reader[SgPartidoOrdinal],
									TotalNotas = reader[TotalNotasOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Despesa:
							IdDespesaOrdinal = reader.GetOrdinal("IdDespesa");
							NomeDespesaOrdinal = reader.GetOrdinal("NomeDespesa");
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
							SgUfOrdinal = reader.GetOrdinal("SgUf");
							DataUltimaNotaFiscalOrdinal = reader.GetOrdinal("DataUltimaNotaFiscal");
							AuditeiOrdinal = reader.GetOrdinal("Auditei");
							DoadorOrdinal = reader.GetOrdinal("Doador");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									Codigo = reader[CodigoOrdinal],
									NomeBeneficiario = reader[NomeBeneficiarioOrdinal],
									SgUf = reader[SgUfOrdinal],
									DataUltimaNotaFiscal = Utils.FormataData(reader[DataUltimaNotaFiscalOrdinal]),
									Auditei = reader[AuditeiOrdinal],
									Doador = reader[DoadorOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}

							sqlSelect.Append(", f.DataUltimaNotaFiscal");
							sqlSelect.Append(", CASE WHEN UserName IS NULL THEN '' ELSE 'Sim' END AS Auditei");
							sqlSelect.Append(", CASE WHEN doador = 1 THEN 'Sim' ELSE '' END AS Doador");
							break;
						case eAgrupamentoAuditoria.Partido:
							SgPartidoOrdinal = reader.GetOrdinal("SgPartido");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									SgPartido = reader[SgPartidoOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Uf:
							SgUfOrdinal = reader.GetOrdinal("SgUf");
							VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

							while (reader.Read())
							{
								lstRetorno.Add(new
								{
									SgUf = reader[SgUfOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							break;
						case eAgrupamentoAuditoria.Documento:
							IdOrdinal = reader.GetOrdinal("Id");
							IdCadastroOrdinal = reader.GetOrdinal("IdCadastro");
							IdDeputadoOrdinal = reader.GetOrdinal("IdDeputado");
							IdDocumentoOrdinal = reader.GetOrdinal("IdDocumento");
							CodigoOrdinal = reader.GetOrdinal("Codigo");
							NumAnoOrdinal = reader.GetOrdinal("NumAno");
							NumeroOrdinal = reader.GetOrdinal("Numero");
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
									IdDeputado = reader[IdDeputadoOrdinal],
									IdDocumento = reader[IdDocumentoOrdinal],
									Codigo = reader[CodigoOrdinal],
									NumAno = reader[NumAnoOrdinal],
									Numero = reader[NumeroOrdinal],
									DataEmissao = Utils.FormataData(reader[DataEmissaoOrdinal]),
									NomeBeneficiario = reader[NomeBeneficiarioOrdinal],
									NomeParlamentar = reader[NomeParlamentarOrdinal],
									VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
								});
							}
							// SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
							// l.txNomeParlamentar as NomeParlamentar");
							// SUM(l.vlrLiquido) AS vlrTotal ");
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

		//private dynamic LancamentosPorDocumento(FiltroParlamentarDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
		//		strSql.Append("l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido, SUM(l.VlrLiquido) AS VlrTotal ");
		//		strSql.Append("FROM camara_lamento l ");
		//		strSql.Append("WHERE l.IdeCadastro > 0 ");
		//		strSql.Append("GROUP BY l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido ");
		//		strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
		//		strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					IdeCadastro = reader[0],
		//					NomeParlamentar = reader[1],
		//					SgUF = reader[2],
		//					SgPartido = reader[3],
		//					VlrTotal = Utils.FormataValor(reader[4])
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				TotalCount = reader[0],
		//				Results = lstRetorno
		//			};
		//		}
		//	}
		//}

		//private dynamic LancamentosPorUf(FiltroParlamentarDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
		//		strSql.Append("l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido, SUM(l.VlrLiquido) AS VlrTotal ");
		//		strSql.Append("FROM camara_lamento l ");
		//		strSql.Append("WHERE l.IdeCadastro > 0 ");
		//		strSql.Append("GROUP BY l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido ");
		//		strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
		//		strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					IdeCadastro = reader[0],
		//					NomeParlamentar = reader[1],
		//					SgUF = reader[2],
		//					SgPartido = reader[3],
		//					VlrTotal = Utils.FormataValor(reader[4])
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				TotalCount = reader[0],
		//				Results = lstRetorno
		//			};
		//		}
		//	}
		//}

		//private dynamic LancamentosPorPartido(FiltroParlamentarDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
		//		strSql.Append("l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido, SUM(l.VlrLiquido) AS VlrTotal ");
		//		strSql.Append("FROM camara_lamento l ");
		//		strSql.Append("WHERE l.IdeCadastro > 0 ");
		//		strSql.Append("GROUP BY l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido ");
		//		strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
		//		strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					IdeCadastro = reader[0],
		//					NomeParlamentar = reader[1],
		//					SgUF = reader[2],
		//					SgPartido = reader[3],
		//					VlrTotal = Utils.FormataValor(reader[4])
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				TotalCount = reader[0],
		//				Results = lstRetorno
		//			};
		//		}
		//	}
		//}

		//private dynamic LancamentosPorFornecedor(FiltroParlamentarDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
		//		strSql.Append("l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido, SUM(l.VlrLiquido) AS VlrTotal ");
		//		strSql.Append("FROM camara_lamento l ");
		//		strSql.Append("WHERE l.IdeCadastro > 0 ");
		//		strSql.Append("GROUP BY l.IdeCadastro, l.NomeParlamentar, l.SgUF, l.SgPartido ");
		//		strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
		//		strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					IdeCadastro = reader[0],
		//					NomeParlamentar = reader[1],
		//					SgUF = reader[2],
		//					SgPartido = reader[3],
		//					VlrTotal = Utils.FormataValor(reader[4])
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				TotalCount = reader[0],
		//				Results = lstRetorno
		//			};
		//		}
		//	}
		//}

		//private dynamic LancamentosPorDespesa(FiltroParlamentarDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
		//		strSql.Append(", l.numSubCota as IdDespesa");
		//		strSql.Append(", l.txtDescricao as Despesa");
		//		strSql.Append(", SUM(l.VlrLiquido) AS VlrTotal ");
		//		strSql.Append("FROM camara_lancamento l ");
		//		strSql.Append("GROUP BY l.txtDescricao ");
		//		strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
		//		strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			int IdDespesaOrdinal = reader.GetOrdinal("IdDespesa");
		//			int DespesaOrdinal = reader.GetOrdinal("Despesa");
		//			int VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					IdDespesa = reader[IdDespesaOrdinal],
		//					Despesa = reader[DespesaOrdinal],
		//					VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				TotalCount = reader[0],
		//				Results = lstRetorno
		//			};
		//		}
		//	}
		//}

		//private dynamic LancamentosPorParlamentar(FiltroParlamentarDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ");
		//		strSql.Append("l.IdeCadastro");
		//		strSql.Append(", l.txNomeParlamentar as NomeParlamentar");
		//		strSql.Append(", p.uf as sgUF");
		//		strSql.Append(", p.partido as SgPartido");
		//		strSql.Append(", COUNT(1) AS TotalNotas");
		//		strSql.Append(", SUM(l.VlrLiquido) AS VlrTotal ");
		//		strSql.Append("FROM lancamentos l ");
		//		strSql.Append("INNER JOIN parlamentares p on p.ideCadastro = l.ideCadastro ");
		//		strSql.Append("WHERE (1=1) ");
		//		strSql.Append("GROUP BY l.IdeCadastro, l.txNomeParlamentar, p.uf, p.partido ");
		//		strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "VlrTotal desc" : filtro.sorting);
		//		strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			int IdeCadastroOrdinal = reader.GetOrdinal("IdeCadastro");
		//			int NomeParlamentarOrdinal = reader.GetOrdinal("NomeParlamentar");
		//			int SgUfOrdinal = reader.GetOrdinal("SgUF");
		//			int SgPartidoOrdinal = reader.GetOrdinal("SgPartido");
		//			int TotalNotasOrdinal = reader.GetOrdinal("TotalNotas");
		//			int VlrTotalOrdinal = reader.GetOrdinal("VlrTotal");

		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					IdeCadastro = reader[IdeCadastroOrdinal],
		//					NomeParlamentar = reader[NomeParlamentarOrdinal],
		//					SgUF = reader[SgUfOrdinal],
		//					SgPartido = reader[SgPartidoOrdinal],
		//					TotalNotas = reader[TotalNotasOrdinal],
		//					VlrTotal = Utils.FormataValor(reader[VlrTotalOrdinal])
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				TotalCount = reader[0],
		//				Results = lstRetorno
		//			};
		//		}
		//	}
		//}

		internal dynamic TipoDespesa(FiltroDropDownDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS numsubcota, txtDescricao FROM despesas ");
				if (!string.IsNullOrEmpty(filtro.q))
				{
					strSql.AppendFormat("WHERE txtDescricao LIKE @q ", filtro.q);
					banco.AddParameter("@q", "%" + filtro.q + "%");
				}
				strSql.AppendFormat("ORDER BY txtDescricao ");
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
				strSql.Append("p.ideCadastro as IdCadastro, p.txNomeParlamentar as NomeParlamentar, count(s.deputado) as Quantidade ");
				strSql.Append("FROM secretario s ");
				strSql.Append("INNER JOIN parlamentares p ON p.ideCadastro = s.deputado ");

				if (!string.IsNullOrEmpty(filtro.NomeParlamentar))
				{
					strSql.AppendFormat("WHERE txNomeParlamentar LIKE '%{0}%' ", filtro.NomeParlamentar);
				}

				strSql.AppendFormat("GROUP BY p.ideCadastro, p.txNomeParlamentar ");

				strSql.AppendFormat("ORDER BY {0} ", string.IsNullOrEmpty(filtro.sorting) ? "Quantidade DESC, p.txNomeParlamentar" : filtro.sorting);
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
				strSql.Append("s.nome, p.txNomeParlamentar as nomeParlamentar ");
				strSql.Append("FROM secretario s ");
				strSql.Append("INNER JOIN parlamentares p on p.ideCadastro = s.deputado ");
				strSql.Append("WHERE p.ideCadastro = @id ");
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