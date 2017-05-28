using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Hosting;
using System.Web.Http;
using Novacode;
using OPS.Core;
using OPS.Models;

namespace OPS.WebApi
{
	[RoutePrefix("Api/CidadaoFiscal")]
	public class CidadaoFiscalController : ApiController
	{
		[HttpGet]
		[Route("")]
		public dynamic Consultar()
		{
			using (var banco = new Banco())
			{
				var lstAuditoriaItem = new List<AuditoriaItem>();
				var lstAuditoriaGrupo = new List<AuditoriaGrupo>();

				using (var reader = banco.ExecuteReader(
						@"SELECT id, nome 
						FROM mcf_tranparencia_grupo;
					
						SELECT 
							id
							, id_mcf_transparencia_grupo
							, informacao_auditada
							, dispositivo_legal 
						FROM mcf_tranparencia_item
						order by id_mcf_transparencia_grupo, id;"
					)
				)
				{
					while (reader.Read())
						lstAuditoriaGrupo.Add(new AuditoriaGrupo
						{
							id = Convert.ToInt32(reader["id"]),
							nome = reader["nome"].ToString(),
							itens = new List<AuditoriaItem>()
						});

					reader.NextResult();

					while (reader.Read())
						lstAuditoriaItem.Add(new AuditoriaItem
						{
							id = Convert.ToInt32(reader["id"]),
							id_grupo = Convert.ToInt32(reader["id_mcf_transparencia_grupo"]),
							informacao_auditada = reader["informacao_auditada"].ToString(),
							dispositivo_legal = reader["dispositivo_legal"].ToString()
						});
				}

				foreach (var grupo in lstAuditoriaGrupo)
				{
					var itens = lstAuditoriaItem.FindAll(o => o.id_grupo == grupo.id);

					grupo.itens.AddRange(itens);
				}

				return new
				{
					grupos = lstAuditoriaGrupo,
					signatarios = new List<AuditoriaSignatario> {new AuditoriaSignatario()}
				};
			}
		}

		[HttpGet]
		[Route("{codigo}")]
		public Auditoria Consultar(string codigo)
		{
			using (var banco = new Banco())
			{
				var id = 0;
				var auditoria = new Auditoria();
				var lstAuditoriaItem = new List<AuditoriaItem>();

				banco.AddParameter("codigo", codigo);
				using (var reader = banco.ExecuteReader(
						@"SELECT 
							id
							, codigo
							, cidade
							, id_estado
							, link_portal
						FROM mcf_auditoria
						where codigo = @codigo"
					)
				)
				{
					if (reader.Read())
					{
						id = Convert.ToInt32(reader["id"]);

						auditoria.grupos = new List<AuditoriaGrupo>();
						auditoria.signatarios = new List<AuditoriaSignatario>();

						auditoria.codigo = reader["codigo"].ToString();
						auditoria.estado = reader["id_estado"].ToString();
						auditoria.cidade = reader["cidade"].ToString();
						auditoria.link_portal = reader["link_portal"].ToString();
					}
					else
					{
						throw new BusinessException("Auditoria não localizada!");
					}
				}

				banco.AddParameter("id", id);
				using (var reader = banco.ExecuteReader(
						@"SELECT 
							id
							, nome 
						FROM mcf_tranparencia_grupo;
					
						SELECT 
							ti.id
							, ti.id_mcf_transparencia_grupo
							, ti.informacao_auditada
							, ti.dispositivo_legal 
							, ai.id_mcf_id_transparencia_item_situacao
							, ai.indicio_de_prova
						FROM mcf_tranparencia_item ti
						left join mcf_auditoria_item ai on ai.id_mcf_id_transparencia_item = ti.id
						where ai.id_mcf_auditoria = @id
						order by id_mcf_transparencia_grupo, id;
						
						SELECT
							sg.id
							, sg.nome
							, sg.cpf
							, sg.rg
							, sg.nacionalidade
							, sg.estado_civil
							, sg.profissao
							, sg.cep
							, sg.endereco
							, sg.bairro
							, sg.cidade
							, sg.id_estado
							, sg.email
						FROM mcf_signatario sg
						inner join mcf_auditoria_signatario asg on asg.id_mcf_signatario = sg.id
						where asg.id_mcf_auditoria = @id;"
					)
				)
				{
					while (reader.Read())
						auditoria.grupos.Add(new AuditoriaGrupo
						{
							id = Convert.ToInt32(reader["id"]),
							nome = reader["nome"].ToString(),
							itens = new List<AuditoriaItem>()
						});

					reader.NextResult();

					while (reader.Read())
						lstAuditoriaItem.Add(new AuditoriaItem
						{
							id = Convert.ToInt32(reader["id"]),
							id_grupo = Convert.ToInt32(reader["id_mcf_transparencia_grupo"]),
							informacao_auditada = reader["informacao_auditada"].ToString(),
							dispositivo_legal = reader["dispositivo_legal"].ToString(),
							situacao = reader["id_mcf_id_transparencia_item_situacao"].ToString(),
							indicio_de_prova = reader["indicio_de_prova"].ToString()
						});

					reader.NextResult();

					while (reader.Read())
						auditoria.signatarios.Add(new AuditoriaSignatario
						{
							//id = Convert.ToInt32(reader["id"]),
							nome_completo = reader["nome"].ToString(),
							cpf = reader["cpf"].ToString(),
							rg = reader["rg"].ToString(),
							nacionalidade = reader["nacionalidade"].ToString(),
							estado_civil = reader["estado_civil"].ToString(),
							profissao = reader["profissao"].ToString(),
							cep = reader["cep"].ToString(),
							endereco = reader["endereco"].ToString(),
							bairro = reader["bairro"].ToString(),
							cidade = reader["cidade"].ToString(),
							estado = reader["id_estado"].ToString(),
							email = reader["email"].ToString()
						});
				}

				foreach (var grupo in auditoria.grupos)
				{
					var itens = lstAuditoriaItem.FindAll(o => o.id_grupo == grupo.id);

					grupo.itens.AddRange(itens);
				}

				return auditoria;
			}
		}

		[HttpPost]
		[Route("")]
		public string Salvar(Auditoria auditoria)
		{
			int? userId;
			try
			{
				var identity = (ClaimsIdentity) User.Identity;
				userId = Convert.ToInt32(identity.FindFirst("UserId").Value);
			}
			catch (Exception)
			{
				userId = null;
			}

			//if (string.IsNullOrEmpty(auditoria.codigo))
			//{
			// TODO: Ajustar para não gerar nova
			auditoria.codigo = Guid.NewGuid().ToString();
			//}

			using (var banco = new Banco())
			{
				banco.BeginTransaction();

				banco.AddParameter("codigo", auditoria.codigo);
				banco.AddParameter("id_user", userId);
				banco.AddParameter("id_estado", auditoria.estado);
				banco.AddParameter("cidade", auditoria.cidade);
				banco.AddParameter("link_portal", auditoria.link_portal);

				var id = banco.ExecuteScalar(
					@"INSERT INTO mcf_auditoria (
						codigo, id_user, data_criacao, data_geracao_denuncia, id_estado, cidade, link_portal
					) VALUES (
						@codigo, @id_user, NOW(), NULL, @id_estado, @cidade, @link_portal
					);
					SELECT LAST_INSERT_ID();");

				var id_mcf_auditoria = Convert.ToInt32(id);


				foreach (var grupo in auditoria.grupos)
				foreach (var item in grupo.itens)
				{
					banco.AddParameter("id_mcf_auditoria", id_mcf_auditoria);
					banco.AddParameter("id_mcf_id_transparencia_item", item.id);
					banco.AddParameter("id_mcf_id_transparencia_item_situacao", item.situacao);
					banco.AddParameter("indicio_de_prova", item.indicio_de_prova);

					banco.ExecuteScalar(
						@"INSERT INTO mcf_auditoria_item (
								id_mcf_auditoria, id_mcf_id_transparencia_item, id_mcf_id_transparencia_item_situacao, indicio_de_prova
							) VALUES (
								@id_mcf_auditoria, @id_mcf_id_transparencia_item, @id_mcf_id_transparencia_item_situacao, @indicio_de_prova
							);");
				}

				foreach (var signatario in auditoria.signatarios)
				{
					banco.AddParameter("nome", signatario.nome_completo);
					banco.AddParameter("cpf", signatario.cpf);
					banco.AddParameter("rg", signatario.rg);
					banco.AddParameter("nacionalidade", signatario.nacionalidade);
					banco.AddParameter("estado_civil", signatario.estado_civil);
					banco.AddParameter("profissao", signatario.profissao);
					banco.AddParameter("cep", signatario.cep);
					banco.AddParameter("endereco", signatario.endereco);
					banco.AddParameter("bairro", signatario.bairro);
					banco.AddParameter("cidade", signatario.cidade);
					banco.AddParameter("id_estado", signatario.estado);
					banco.AddParameter("email", signatario.email);

					var id_mcf_signatario = banco.ExecuteScalar(
						@"INSERT INTO mcf_signatario (
								nome, cpf, rg, nacionalidade, estado_civil, profissao, cep, endereco, bairro, cidade, id_estado, email
							) VALUES (
								@nome, @cpf, @rg, @nacionalidade, @estado_civil, @profissao, @cep, @endereco, @bairro, @cidade, @id_estado, @email
							);
							SELECT LAST_INSERT_ID();");

					banco.AddParameter("id_mcf_auditoria", id_mcf_auditoria);
					banco.AddParameter("id_mcf_signatario", Convert.ToInt32(id_mcf_signatario));

					banco.ExecuteScalar(
						@"INSERT INTO mcf_auditoria_signatario (
								id_mcf_auditoria, id_mcf_signatario
							) VALUES (
								@id_mcf_auditoria, @id_mcf_signatario
							);");
				}

				banco.CommitTransaction();
			}

			return auditoria.codigo;
		}

		[HttpGet]
		[Route("GerarDenuncia/{codigo}")]
		public HttpResponseMessage GerarDenuncia(string codigo)
		{
			var auditoria = Consultar(codigo);

			// Store a global reference to the loaded document.
			var g_document = DocX.Load(HostingEnvironment.MapPath("~/temp/Template Cidadao Fiscal.docx"));

			// Replace text in this document.
			g_document.ReplaceText("#CIDADE_AUDITADA#", auditoria.cidade);
			g_document.ReplaceText("#DATA_EXTENSO#", DateTime.Now.ToString(@"dd \de MMMM \de yyyy", new CultureInfo("pt-BR")));

			var auditor = auditoria.signatarios[0];
			g_document.ReplaceText("#DADOS_DO_AUDITOR#",
				string.Concat(auditor.nacionalidade, ", ", //TODO: converter para texto
					auditor.estado_civil, ", ", //TODO: converter para texto
					auditor.profissao, ", ",
					"portador da carteira de identidade nº ", auditor.rg, ", ",
					"inscrito no CPF nº ", auditor.cpf, ", ",
					"residente e domiciliado na ", auditor.endereco, ", ",
					"CEP ", auditor.cep, ", ",
					"Bairro ", auditor.bairro, ", ",
					"Cidade de ", auditor.cidade, ", ",
					auditor.estado)); //TODO: converter para texto

			g_document.ReplaceText("#CIDADE_AUDITOR#", auditor.cidade);
			g_document.ReplaceText("#NOME_AUDITOR#", auditor.nome_completo);
			g_document.ReplaceText("#CPF_AUDITOR#", auditor.cpf);

			var t = g_document.Tables[0];

			var title_formatting = new Formatting
			{
				Bold = true,
				Size = 12,
				FontFamily = new FontFamily("Cambria")
			};

			var item_title_formatting = new Formatting
			{
				Bold = true,
				Size = 12,
				FontFamily = new FontFamily("Cambria")
			};

			var item_text_formatting = new Formatting
			{
				Size = 12,
				FontFamily = new FontFamily("Cambria")
			};

			var grupos = auditoria.grupos.FindAll(o => o.itens.Any(i => i.situacao == "2" || i.situacao == "3"));

			var table = t.InsertTableAfterSelf(grupos.Count, 1);
			table.Design = TableDesign.TableNormal;
			table.AutoFit = AutoFit.Window;

			for (var g = 0; g < grupos.Count; g++)
			{
				var itens = grupos[g].itens.FindAll(i => i.situacao == "2" || i.situacao == "3");
				var cell = table.Rows[g].Cells[0];

				var cellParagraphGrupo = cell.Paragraphs[0];
				cellParagraphGrupo.InsertText($"{g + 1:00}. {grupos[g].nome.ToUpper()}", false, title_formatting);
				cellParagraphGrupo.SetLineSpacing(LineSpacingType.Before, 1);
				cellParagraphGrupo.SetLineSpacing(LineSpacingType.After, 1);

				foreach (var item in itens)
				{
					var tbItens = cell.InsertTable(3, 1);
					tbItens.AutoFit = AutoFit.Window;
					tbItens.Design = TableDesign.TableGrid;

					var situacao = string.Empty;
					switch (item.situacao)
					{
						case "2":
							situacao = "Atende parcialmente";
							break;
						case "3":
							situacao = "Informação não encontrada";
							break;
					}

					var cellParagraphItemIa = tbItens.Rows[0].Cells[0].Paragraphs[0];
					cellParagraphItemIa.SetLineSpacing(LineSpacingType.After, 0);
					cellParagraphItemIa.InsertText(situacao + ": ", false, item_title_formatting);
					cellParagraphItemIa.InsertText(item.informacao_auditada, false, item_text_formatting);

					var cellParagraphItemIp = tbItens.Rows[1].Cells[0].Paragraphs[0];
					cellParagraphItemIp.SetLineSpacing(LineSpacingType.After, 0);
					cellParagraphItemIp.InsertText("Indício de Prova: ", false, item_title_formatting);
					cellParagraphItemIp.InsertText(item.indicio_de_prova, false, item_text_formatting);

					var cellParagraphItemDl = tbItens.Rows[2].Cells[0].Paragraphs[0];
					cellParagraphItemDl.SetLineSpacing(LineSpacingType.After, 0);
					cellParagraphItemDl.InsertText("Dispositivo Legal: ", false, item_title_formatting);
					cellParagraphItemDl.InsertText(!string.IsNullOrEmpty(item.dispositivo_legal) ? item.dispositivo_legal : "N/A",
						false, item_text_formatting);
				}
			}

			foreach (var p in t.Paragraphs)
			{
				p.SetLineSpacing(LineSpacingType.Before, 0);
				p.SetLineSpacing(LineSpacingType.After, 0);
			}

			t.Remove();

			var stream = new MemoryStream();
			g_document.SaveAs(stream);
			g_document.Dispose();

			// processing the stream.
			var result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(stream.ToArray())
			};
			result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = $"Cidadão Fiscal - {auditoria.cidade}.docx"
			};
			result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

			return result;
		}
	}
}