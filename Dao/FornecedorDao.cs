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
	}
}