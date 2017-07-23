using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using OPS.Core.DTO;
using OPS.Core.Models;

namespace OPS.Core.DAO
{
	public class DenunciaDao
	{
		public dynamic Consultar(FiltroDenunciaDTO filtro, string userId)
		{
			using (var banco = new Banco())
			{
				var strSql = new StringBuilder();
				strSql.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
	                    d.id as id_denuncia,
	                    d.codigo,
                        f.cnpj_cpf,
                        f.nome as nome_fornecedor,
                        ud.FullName as nome_usuario_denuncia,
                        d.data_denuncia,
                        case d.situacao 
							when 'A' then 'Aguardando Revisão'
                            when 'I' then 'Pendente Informação'
                            when 'P' then 'Caso Duvidoso'
                            when 'D' then 'Caso Dossiê'
                            when 'R' then 'Caso Repetido'
                            when 'N' then 'Não Procede'
						end as situacao,
                        d.data_auditoria,
                        ua.FullName as nome_usuario_auditoria
                    FROM denuncia d
                    inner join fornecedor f on f.id = d.id_fornecedor
                    inner join users ud on ud.id = d.id_user_denuncia
                    inner join users ua on ua.id = d.id_user_auditoria
                    WHERE (1=1)
				");

				using (var repository = new AuthRepository())
				{
					var bRevisor = repository.IsInRoleAsync(userId, "Revisor");
					if (!bRevisor.Result)
					{
						banco.AddParameter("id_user_denuncia", userId);
						strSql.AppendFormat(" AND d.id_user_denuncia like @id_user_denuncia ");
					}
				}

				var lstSituacoes = new List<string>();
				if (filtro.AguardandoRevisao) lstSituacoes.Add("'A'");
				if (filtro.PendenteInformacao) lstSituacoes.Add("'I'");
				if (filtro.Duvidoso) lstSituacoes.Add("'P'");
				if (filtro.Dossie) lstSituacoes.Add("'D'");
				if (filtro.Repetido) lstSituacoes.Add("'R'");
				if (filtro.NaoProcede) lstSituacoes.Add("'N'");

				if (lstSituacoes.Count > 0)
					strSql.AppendFormat(" AND d.situacao in({0}) ", string.Join(",", lstSituacoes));

				strSql.AppendFormat("ORDER BY {0} ",
					string.IsNullOrEmpty(filtro.sorting) ? "id_denuncia ASC" : Utils.MySqlEscape(filtro.sorting));
				strSql.AppendFormat("LIMIT {0},{1}; ", (filtro.page - 1) * filtro.count, filtro.count);

				strSql.AppendLine("SELECT FOUND_ROWS(); ");

				var lstRetorno = new List<dynamic>();
				using (var reader = banco.ExecuteReader(strSql.ToString()))
				{
					while (reader.Read())
						lstRetorno.Add(new
						{
							id_denuncia = reader["id_denuncia"],
							codigo = reader["codigo"],
							cnpj_cpf = reader["cnpj_cpf"].ToString(),
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							nome_usuario_denuncia = reader["nome_usuario_denuncia"].ToString(),
							data_denuncia = Utils.FormataDataHora(reader["data_denuncia"]),
							situacao = reader["situacao"].ToString(),
							data_auditoria = Utils.FormataDataHora(reader["data_auditoria"]),
							nome_usuario_auditoria = reader["nome_usuario_auditoria"].ToString()
						});

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

		public DenunciaModel Consultar(string value)
		{
			string strSqlDenuncia = @"SELECT 
	                        d.id as id_denuncia,
                            d.codigo,
                            d.id_fornecedor,
                            f.cnpj_cpf,
                            f.nome as nome_fornecedor,
                            ud.FullName as nome_usuario_denuncia,
                            d.data_denuncia,
                            d.texto,
                            d.anexo,
                            d.situacao,
                            case d.situacao 
							    when 'A' then 'Aguardando Revisão'
                                when 'I' then 'Pendente Informação'
                                when 'P' then 'Caso Duvidoso'
                                when 'D' then 'Caso Dossiê'
                                when 'R' then 'Caso Repetido'
                                when 'N' then 'Não Procede'
						    end as situacao_descricao,
                            d.data_auditoria,
                            ua.FullName as nome_usuario_auditoria
                        FROM denuncia d
                        inner join fornecedor f on f.id = d.id_fornecedor
                        inner join users ud on ud.id = d.id_user_denuncia
                        inner join users ua on ua.id = d.id_user_auditoria
                        where d.id = @id;";

			string strSqlDenunciaAnexo = @"SELECT 
	                        da.id,
                            ud.FullName as nome_usuario,
                            da.data,
                            da.nome_arquivo
                        FROM denuncia_anexo da
                        inner join users ud on ud.id = da.id_user
                        where id_denuncia = @id;";

			string strSqlDenunciaMensagem = @"SELECT dm.id,
                            ud.FullName as nome_usuario,
                            dm.data,
                            dm.texto
                        FROM denuncia_mensagem dm
                        inner join users ud on ud.id = dm.id_user
                        where id_denuncia = @id;";

			//TODO: Quem possui o link pode interragir na denuncia?
			//using (var repository = new AuthRepository())
			//{
			//    var bRevisor = repository.IsInRoleAsync(userId, "Revisor");
			//    if (!bRevisor.Result)
			//    {
			//        banco.AddParameter("id_user_denuncia", userId);
			//        strSql.AppendFormat(" AND d.id_user_denuncia like @id_user_denuncia ");
			//    }
			//}

			using (var banco = new Banco())
			{
				DenunciaModel denuncia;

				banco.AddParameter("id", value);
				using (var reader = banco.ExecuteReader(strSqlDenuncia))
				{
					if (reader.Read())
					{
						denuncia = new DenunciaModel()
						{
							id_denuncia = reader["id_denuncia"].ToString(),
							codigo = Convert.ToInt32(reader["codigo"]),
							cnpj_cpf = reader["cnpj_cpf"].ToString(),
							id_fornecedor = Convert.ToInt32(reader["id_fornecedor"]),
							nome_fornecedor = reader["nome_fornecedor"].ToString(),
							nome_usuario_denuncia = reader["nome_usuario_denuncia"].ToString(),
							data_denuncia = Utils.FormataDataHora(reader["data_denuncia"]),
							texto = reader["texto"].ToString(),
							anexo = reader["anexo"].ToString(),
							situacao = reader["situacao"].ToString(),
							situacao_descricao = reader["situacao_descricao"].ToString(),
							data_auditoria = Utils.FormataDataHora(reader["data_auditoria"]),
							nome_usuario_auditoria = reader["nome_usuario_auditoria"].ToString()
						};
					}
					else
					{
						return null;
					}
				}

				banco.AddParameter("id", value);
				using (var reader = banco.ExecuteReader(strSqlDenunciaAnexo))
				{
					if (reader.HasRows)
					{
						denuncia.anexos = new List<DenunciaAnexoModel>();

						while (reader.Read())
						{
							denuncia.anexos.Add(new DenunciaAnexoModel()
							{
								nome_usuario = reader["nome_usuario"].ToString(),
								data = Utils.FormataDataHora(reader["data"]),
								nome_arquivo = reader["nome_arquivo"].ToString()
							});
						}
					}
				}

				banco.AddParameter("id", value);
				using (var reader = banco.ExecuteReader(strSqlDenunciaMensagem))
				{
					if (reader.HasRows)
					{
						denuncia.mensagens = new List<DenunciaMensagemModel>();

						while (reader.Read())
						{
							denuncia.mensagens.Add(new DenunciaMensagemModel()
							{
								nome_usuario = reader["nome_usuario"].ToString(),
								data = Utils.FormataDataHora(reader["data"]),
								texto = reader["texto"].ToString()
							});
						}
					}
				}

				return denuncia;
			}
		}

		public async void AdicionarComentario(DenunciaComentarioDTO value, string userId, string userFullName)
		{
			string codigoDenuncia, situacaoDenuncia, cnpjCpf, razaoSocial;
			var lstDestinatarios = new List<MailAddress>();

			using (var banco = new Banco())
			{
				banco.AddParameter("id", value.id_denuncia);
				using (var reader = banco.ExecuteReader(
					@"select d.codigo, d.situacao, f.cnpj_cpf, f.nome as razao_social from denuncia d
                    inner join fornecedor f on f.id = d.id_fornecedor
                    where d.id=@id;"))
				{
					if (reader.Read())
					{
						codigoDenuncia = reader["codigo"].ToString();
						situacaoDenuncia = reader["situacao"].ToString();
						cnpjCpf = reader["cnpj_cpf"].ToString();
						razaoSocial = reader["razao_social"].ToString();
					}
					else
					{
						throw new BusinessException("Não foi possivel localizar a denúncia para adicionar seu comentário!");
					}
				}

				if (!string.IsNullOrEmpty(value.situacao) && situacaoDenuncia != value.situacao)
				{
					using (var repository = new AuthRepository())
					{
						var bRevisor = repository.IsInRoleAsync(userId, "Revisor");
						if (bRevisor.Result)
						{
							banco.AddParameter("situacao", value.situacao);
							banco.AddParameter("id_user_auditoria", userId);
							banco.AddParameter("id", value.id_denuncia);
							banco.ExecuteNonQuery(
								@"update denuncia set 
                                    situacao=@situacao, 
                                    id_user_auditoria=@id_user_auditoria, 
                                    data_auditoria=NOW() 
                                where id=@id");
						}
					}

					var situacaoDescricao = "";
					switch (value.situacao)
					{
						case "A":
							situacaoDescricao = "Aguardando Revisão";
							break;
						case "I":
							situacaoDescricao = "Pendente Informação";
							break;
						case "P":
							situacaoDescricao = "Caso Duvidoso";
							break;
						case "D":
							situacaoDescricao = "Caso Dossiê";
							break;
						case "R":
							situacaoDescricao = "Caso Repetido";
							break;
						case "N":
							situacaoDescricao = "Não Procede";
							break;
					}
					value.texto += " [Situação alterada para " + situacaoDescricao + "]";
				}

				banco.AddParameter("id", Guid.NewGuid().ToString());
				banco.AddParameter("id_denuncia", value.id_denuncia);
				banco.AddParameter("id_user", userId);
				banco.AddParameter("texto", value.texto);
				banco.ExecuteNonQuery(
					@"INSERT INTO denuncia_mensagem (id, id_denuncia, id_user, data, texto) 
                        VALUES (@id, @id_denuncia, @id_user, NOW(), @texto);");


				banco.AddParameter("id", value.id_denuncia);
				banco.AddParameter("id_user", userId);
				using (var reader = banco.ExecuteReader(
					@"SELECT u.Email, u.Fullname FROM (
	                    SELECT id_user_denuncia as id_user 
	                    FROM denuncia
	                    WHERE id=@id
	                    union
	                    SELECT id_user_auditoria FROM denuncia
	                    WHERE id=@id
	                    union
	                    SELECT id_user FROM denuncia_mensagem
	                    WHERE id_denuncia=@id
                    ) d
                    inner join users u on u.id = d.id_user
                    where u.id <> @id_user;"))
				{
					while (reader.Read())
					{
						lstDestinatarios.Add(new MailAddress(reader["Email"].ToString(), reader["Fullname"].ToString()));
					}
				}
			}

			if (lstDestinatarios.Count > 0)
			{
				StringBuilder corpo = new StringBuilder();

				corpo.Append(@"<html><head><title>O.P.S.</title></head><body><table width=""100%""><tr><td><center><h3>O.P.S. - Operação Política Supervisionada</h3></center></td></tr><tr><td><i>Um novo comentário foi adicionado a sua denúncia.</i></td></tr><tr><td><table><tr><td valign=""top""><b>Denúncia:</b></td><td>");
				corpo.AppendFormat(@"<a href=""http://www.ops.net.br/denuncia/{0}"">{1}</a></td></tr>", value.id_denuncia, codigoDenuncia);
				corpo.AppendFormat(@"<tr><td valign=""top""><b>Fornecedor:</b></td><td>{0} - {1}</td></tr>", cnpjCpf, razaoSocial);
				corpo.AppendFormat(@"<tr><td valign=""top""><b>Usuário:</b></td><td>{0}</td></tr>", userFullName);
				corpo.AppendFormat(@"<tr><td valign=""top""><b>Texto:</b></td><td>{0}</td></tr></table></td></tr></table></body></html>", value.texto);

				foreach (MailAddress destinatario in lstDestinatarios)
				{
					await Utils.SendMailAsync(destinatario, "[O.P.S.] Novo Comentário", corpo.ToString());
				}
			}
		}
	}
}