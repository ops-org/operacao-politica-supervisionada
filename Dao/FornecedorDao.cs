using MySql.Data.MySqlClient;
using OPS.Core;
using System.Collections.Generic;
using System.Text;
using System;
using OPS.Models;

namespace OPS.Dao
{
	public class FornecedorDao
	{
		internal dynamic Pesquisa(FiltroDropDownDTO filtro)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT SQL_CALC_FOUND_ROWS txtCNPJCPF, txtBeneficiario FROM fornecedores ");

				if (!string.IsNullOrEmpty(filtro.q))
				{
					strSql.AppendFormat("WHERE txtBeneficiario LIKE @q OR txtCNPJCPF LIKE @q ", filtro.q);
					banco.AddParameter("@q", "%" + filtro.q + "%");
				}
				else if (!string.IsNullOrEmpty(filtro.qs))
				{
					strSql.AppendFormat("WHERE txtCNPJCPF IN({0}) ", "'" + filtro.qs.Replace(",", "','") + "'");
				}

				strSql.AppendFormat("ORDER BY txtBeneficiario, Uf ");
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
							text = string.Format("{0} ({1})", reader[1].ToString(), reader[0].ToString()),
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

		internal Fornecedor Consulta(string cnpj)
		{
			using (Banco banco = new Banco())
			{
				banco.AddParameter("Cnpj", cnpj);

				using (MySqlDataReader reader = banco.ExecuteReader("SELECT * FROM fornecedores WHERE txtCNPJCPF = @Cnpj"))
				{
					var fornecedor = new Fornecedor();
					if (reader.Read())
					{
						fornecedor.CnpjCpf = reader["txtCNPJCPF"].ToString();
						fornecedor.DataAbertura = Utils.FormataData(reader["DataAbertura"]);
						fornecedor.RazaoSocial = reader["txtBeneficiario"].ToString();
						fornecedor.NomeFantasia = reader["NomeFantasia"].ToString();
						fornecedor.AtividadePrincipal = reader["AtividadePrincipal"].ToString();
						fornecedor.NaturezaJuridica = reader["NaturezaJuridica"].ToString();
						fornecedor.Logradouro = reader["Logradouro"].ToString();
						fornecedor.Numero = reader["Numero"].ToString();
						fornecedor.Complemento = reader["Complemento"].ToString();
						fornecedor.Cep = reader["Cep"].ToString();
						fornecedor.Bairro = reader["Bairro"].ToString();
						fornecedor.Cidade = reader["Cidade"].ToString();
						fornecedor.Uf = reader["Uf"].ToString();
						fornecedor.Situacao = reader["Situacao"].ToString();
						fornecedor.DataSituacao = reader["DataSituacao"].ToString();
						fornecedor.MotivoSituacao = reader["MotivoSituacao"].ToString();
						fornecedor.SituacaoEspecial = reader["SituacaoEspecial"].ToString();
						fornecedor.Email = reader["Email"].ToString();
						fornecedor.Telefone = reader["Telefone"].ToString();
						fornecedor.EnteFederativoResponsavel = reader["EnteFederativoResponsavel"].ToString();
						fornecedor.Doador = !Convert.IsDBNull(reader["Doador"]) ? Convert.ToBoolean(reader["Doador"]) : false;
						fornecedor.DataSituacaoEspecial = reader["DataSituacaoEspecial"].ToString();
						fornecedor.DataInclusao = Utils.FormataDataHora(reader["DataInclusao"]);
						fornecedor.CapitalSocial = reader["CapitalSocial"].ToString();

						var lstAtividadeSecundaria = new List<string>();
						for (int i = 1; i <= 20; i++)
						{
							if (!string.IsNullOrEmpty(reader["AtividadeSecundaria" + i.ToString("00")].ToString()))
							{
								lstAtividadeSecundaria.Add(reader["AtividadeSecundaria" + i.ToString("00")].ToString());
							}
						}
						fornecedor.AtividadeSecundaria = string.Join("<br/>", lstAtividadeSecundaria);
					}

					return fornecedor;
				}
			}
		}

		internal List<FornecedorQuadroSocietario> QuadroSocietario(string cnpj)
		{
			try
			{
				var lstFornecedorQuadroSocietario = new List<FornecedorQuadroSocietario>();

				using (Banco banco = new Banco())
				{
					banco.AddParameter("Cnpj", cnpj);

					using (MySqlDataReader reader = banco.ExecuteReader("SELECT * FROM FornecedorQuadroSocietario WHERE txtCNPJCPF = @Cnpj"))
					{
						while (reader.Read())
						{
							var fornecedorQuadroSocietario = new FornecedorQuadroSocietario();

							try { fornecedorQuadroSocietario.Nome = Convert.ToString(reader["Nome"]); }
							catch { fornecedorQuadroSocietario.Nome = ""; }

							try { fornecedorQuadroSocietario.Qualificacao = Convert.ToString(reader["Qualificacao"]); }
							catch { fornecedorQuadroSocietario.Qualificacao = ""; }

							try { fornecedorQuadroSocietario.NomeRepresentanteLegal = Convert.ToString(reader["NomeRepresentanteLegal"]); }
							catch { fornecedorQuadroSocietario.NomeRepresentanteLegal = ""; }

							try { fornecedorQuadroSocietario.QualificacaoRepresentanteLegal = Convert.ToString(reader["QualificacaoRepresentanteLegal"]); }
							catch { fornecedorQuadroSocietario.QualificacaoRepresentanteLegal = ""; }

							lstFornecedorQuadroSocietario.Add(fornecedorQuadroSocietario);
						}

						return lstFornecedorQuadroSocietario;
					}
				}
			}
			catch (Exception)
			{ } //TODO: logar erro

			return null;
		}

		public Boolean AtualizaDados(Fornecedor fornecedor)
		{
			using (Banco banco = new Banco())
			{
				banco.AddParameter("DataAbertura", fornecedor.DataAbertura);
				banco.AddParameter("RazaoSocial", fornecedor.RazaoSocial);
				banco.AddParameter("NomeFantasia", fornecedor.NomeFantasia);
				banco.AddParameter("AtividadePrincipal", fornecedor.AtividadePrincipal);
				banco.AddParameter("NaturezaJuridica", fornecedor.NaturezaJuridica);
				banco.AddParameter("Logradouro", fornecedor.Logradouro);
				banco.AddParameter("Numero", fornecedor.Numero);
				banco.AddParameter("Complemento", fornecedor.Complemento);
				banco.AddParameter("Cep", fornecedor.Cep);
				banco.AddParameter("Bairro", fornecedor.Bairro);
				banco.AddParameter("Cidade", fornecedor.Cidade);
				banco.AddParameter("Uf", fornecedor.Uf);
				banco.AddParameter("Situacao", fornecedor.Situacao);
				banco.AddParameter("DataSituacao", fornecedor.DataSituacao);
				banco.AddParameter("MotivoSituacao", fornecedor.MotivoSituacao);
				banco.AddParameter("SituacaoEspecial", fornecedor.SituacaoEspecial);
				banco.AddParameter("DataSituacaoEspecial", fornecedor.DataSituacaoEspecial);

				banco.AddParameter("Email", fornecedor.Email);
				banco.AddParameter("Telefone", fornecedor.Telefone);
				banco.AddParameter("EnteFederativoResponsavel", fornecedor.EnteFederativoResponsavel);

				banco.AddParameter("AtividadeSecundaria01", fornecedor.AtividadeSecundaria01);
				banco.AddParameter("AtividadeSecundaria02", fornecedor.AtividadeSecundaria02);
				banco.AddParameter("AtividadeSecundaria03", fornecedor.AtividadeSecundaria03);
				banco.AddParameter("AtividadeSecundaria04", fornecedor.AtividadeSecundaria04);
				banco.AddParameter("AtividadeSecundaria05", fornecedor.AtividadeSecundaria05);
				banco.AddParameter("AtividadeSecundaria06", fornecedor.AtividadeSecundaria06);
				banco.AddParameter("AtividadeSecundaria07", fornecedor.AtividadeSecundaria07);
				banco.AddParameter("AtividadeSecundaria08", fornecedor.AtividadeSecundaria08);
				banco.AddParameter("AtividadeSecundaria09", fornecedor.AtividadeSecundaria09);
				banco.AddParameter("AtividadeSecundaria10", fornecedor.AtividadeSecundaria10);
				banco.AddParameter("AtividadeSecundaria11", fornecedor.AtividadeSecundaria11);
				banco.AddParameter("AtividadeSecundaria12", fornecedor.AtividadeSecundaria12);
				banco.AddParameter("AtividadeSecundaria13", fornecedor.AtividadeSecundaria13);
				banco.AddParameter("AtividadeSecundaria14", fornecedor.AtividadeSecundaria14);
				banco.AddParameter("AtividadeSecundaria15", fornecedor.AtividadeSecundaria15);
				banco.AddParameter("AtividadeSecundaria16", fornecedor.AtividadeSecundaria16);
				banco.AddParameter("AtividadeSecundaria17", fornecedor.AtividadeSecundaria17);
				banco.AddParameter("AtividadeSecundaria18", fornecedor.AtividadeSecundaria18);
				banco.AddParameter("AtividadeSecundaria19", fornecedor.AtividadeSecundaria19);
				banco.AddParameter("AtividadeSecundaria20", fornecedor.AtividadeSecundaria20);
				banco.AddParameter("CapitalSocial", fornecedor.CapitalSocial);
				banco.AddParameter("UsuarioInclusao", fornecedor.UsuarioInclusao);
				banco.AddParameter("Cnpj", fornecedor.CnpjCpf);

				System.Text.StringBuilder sql = new System.Text.StringBuilder();

				sql.Append("UPDATE fornecedores");
				sql.Append("   SET txtBeneficiario       = @RazaoSocial,");
				sql.Append("       NomeFantasia          = @NomeFantasia,");
				sql.Append("       AtividadePrincipal    = @AtividadePrincipal,");
				sql.Append("       NaturezaJuridica      = @NaturezaJuridica,");
				sql.Append("       Logradouro            = @Logradouro,");
				sql.Append("       Numero                = @Numero,");
				sql.Append("       Complemento           = @Complemento,");
				sql.Append("       Cep                   = @Cep,");
				sql.Append("       Bairro                = @Bairro,");
				sql.Append("       Cidade                = @Cidade,");
				sql.Append("       Uf                    = @Uf,");
				sql.Append("       Situacao              = @Situacao,");
				sql.Append("       DataSituacao          = @DataSituacao,");
				sql.Append("       MotivoSituacao        = @MotivoSituacao,");
				sql.Append("       SituacaoEspecial      = @SituacaoEspecial,");
				sql.Append("       DataSituacaoEspecial  = @DataSituacaoEspecial,");
				sql.Append("       DataAbertura          = @DataAbertura,");

				sql.Append("       Email                = @Email,");
				sql.Append("       Telefone             = @Telefone,");
				sql.Append("       EnteFederativoResponsavel = @EnteFederativoResponsavel,");

				sql.Append("       AtividadeSecundaria01 = @AtividadeSecundaria01,");
				sql.Append("       AtividadeSecundaria02 = @AtividadeSecundaria02,");
				sql.Append("       AtividadeSecundaria03 = @AtividadeSecundaria03,");
				sql.Append("       AtividadeSecundaria04 = @AtividadeSecundaria04,");
				sql.Append("       AtividadeSecundaria05 = @AtividadeSecundaria05,");
				sql.Append("       AtividadeSecundaria05 = @AtividadeSecundaria06,");
				sql.Append("       AtividadeSecundaria07 = @AtividadeSecundaria07,");
				sql.Append("       AtividadeSecundaria08 = @AtividadeSecundaria08,");
				sql.Append("       AtividadeSecundaria09 = @AtividadeSecundaria09,");
				sql.Append("       AtividadeSecundaria10 = @AtividadeSecundaria10,");
				sql.Append("       AtividadeSecundaria11 = @AtividadeSecundaria11,");
				sql.Append("       AtividadeSecundaria12 = @AtividadeSecundaria12,");
				sql.Append("       AtividadeSecundaria13 = @AtividadeSecundaria13,");
				sql.Append("       AtividadeSecundaria14 = @AtividadeSecundaria14,");
				sql.Append("       AtividadeSecundaria15 = @AtividadeSecundaria15,");
				sql.Append("       AtividadeSecundaria15 = @AtividadeSecundaria16,");
				sql.Append("       AtividadeSecundaria17 = @AtividadeSecundaria17,");
				sql.Append("       AtividadeSecundaria18 = @AtividadeSecundaria18,");
				sql.Append("       AtividadeSecundaria19 = @AtividadeSecundaria19,");
				sql.Append("       AtividadeSecundaria20 = @AtividadeSecundaria20,");
				sql.Append("       CapitalSocial         = @CapitalSocial,");
				sql.Append("       UsuarioInclusao       = @UsuarioInclusao,");
				sql.Append("       DataInclusao          = NOW()");
				sql.Append(" WHERE txtCNPJCPF            = @Cnpj");

				if (banco.ExecuteNonQuery(sql.ToString()) == false)
				{
					return false;
				}

				if (fornecedor.lstFornecedorQuadroSocietario != null)
				{
					banco.AddParameter("txtCNPJCPF", fornecedor.CnpjCpf);
					banco.ExecuteScalar("DELETE FROM fornecedorquadrosocietario WHERE txtCNPJCPF = @txtCNPJCPF");

					foreach (var qas in fornecedor.lstFornecedorQuadroSocietario)
					{
						banco.AddParameter("txtCNPJCPF", fornecedor.CnpjCpf);
						banco.AddParameter("Nome", qas.Nome);
						banco.AddParameter("Qualificacao", qas.Qualificacao);
						banco.AddParameter("NomeRepresentanteLegal", qas.NomeRepresentanteLegal);
						banco.AddParameter("QualificacaoRepresentanteLegal", qas.QualificacaoRepresentanteLegal);

						banco.ExecuteNonQuery(
							"INSERT fornecedorquadrosocietario (txtCNPJCPF, Nome, Qualificacao, NomeRepresentanteLegal, QualificacaoRepresentanteLegal) VALUES " +
							"(@txtCNPJCPF, @Nome, @Qualificacao, @NomeRepresentanteLegal, @QualificacaoRepresentanteLegal)");
					}
				}
			}

			return true;
		}

		internal dynamic SenadoresMaioresGastos(string value)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT");
				strSql.Append(" SUM(l.Valor) AS VlrLiquido");
				strSql.Append(", l.CodigoParlamentar AS IdCadastro");
				strSql.Append(", p.NomeParlamentar");
				strSql.Append(" FROM lancamentos_senadores l ");
				strSql.Append(" LEFT JOIN senadores p ON p.CodigoParlamentar = l.CodigoParlamentar");
				strSql.Append(" WHERE l.CnpjCpf = @value");
				strSql.Append(" GROUP BY IdCadastro, NomeParlamentar");
				strSql.Append(" ORDER BY VlrLiquido desc");
				strSql.Append(" LIMIT 10");
				banco.AddParameter("@value", value);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							IdCadastro = reader["IdCadastro"].ToString(),
							NomeParlamentar = reader["NomeParlamentar"].ToString(),
							VlrLiquido = Utils.FormataValor(reader["VlrLiquido"])
						});
					}

					return lstRetorno;
				}
			}
		}

		internal dynamic DeputadoFederalMaioresGastos(string value)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT");
				strSql.Append(" SUM(l.VlrLiquido) AS VlrLiquido");
				strSql.Append(", l.ideCadastro AS IdCadastro");
				strSql.Append(", p.txNomeParlamentar as NomeParlamentar");
				strSql.Append(" FROM lancamentos l ");
				strSql.Append(" LEFT JOIN parlamentares p ON p.ideCadastro = l.ideCadastro");
				strSql.Append(" WHERE l.txtCNPJCPF = @value");
				strSql.Append(" GROUP BY IdCadastro, NomeParlamentar");
				strSql.Append(" ORDER BY VlrLiquido desc");
				strSql.Append(" LIMIT 10");
				banco.AddParameter("@value", value);

				using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
				{
					List<dynamic> lstRetorno = new List<dynamic>();
					while (reader.Read())
					{
						lstRetorno.Add(new
						{
							IdCadastro = reader["IdCadastro"].ToString(),
							NomeParlamentar = reader["NomeParlamentar"].ToString(),
							VlrLiquido = Utils.FormataValor(reader["VlrLiquido"])
						});
					}

					return lstRetorno;
				}
			}
		}

		internal dynamic RecebimentosMensaisPorAno(string value)
		{
			using (Banco banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.Append("SELECT l.numAno, l.numMes, SUM(l.VlrLiquido) AS VlrTotal ");
				strSql.Append("FROM lancamentos l ");
				strSql.Append("WHERE l.txtCNPJCPF = @value ");
				strSql.Append("group by l.numAno, l.numMes ");
				strSql.Append("order by l.numAno, l.numMes ");
				banco.AddParameter("@value", value);

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

		public void MarcaVisitado(string Cnpj, string UserName)
		{
			using (Banco banco = new Banco())
			{
				banco.AddParameter("txtCNPJCPF", Cnpj);
				banco.AddParameter("UserName", UserName);

				try
				{
					banco.ExecuteNonQuery("INSERT INTO fornecedores_visitado (txtCNPJCPF, UserName) VALUES (@txtCNPJCPF, @UserName)");
				}
				catch { }
			}
		}

	}
}