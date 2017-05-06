using DocumentFormat.OpenXml.Packaging;
using MySql.Data.MySqlClient;
using OPS.Core;
using OPS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Http;
using System.Linq;
using System.Web.Hosting;
using OPS.Core.CidadaoFiscal;

namespace OPS.WebApi
{
	[RoutePrefix("Api/CidadaoFiscal")]
	public class CidadaoFiscalController : ApiController
	{
		[HttpGet]
		[Route("")]
		public dynamic Consultar()
		{
			using (Banco banco = new Banco())
			{
				var lstAuditoriaItem = new List<AuditoriaItem>();
				var lstAuditoriaGrupo = new List<AuditoriaGrupo>();

				using (MySqlDataReader reader = banco.ExecuteReader(
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
					{
						lstAuditoriaGrupo.Add(new AuditoriaGrupo()
						{
							id = Convert.ToInt32(reader["id"]),
							nome = reader["nome"].ToString(),
							itens = new List<AuditoriaItem>()
						});
					}

					reader.NextResult();

					while (reader.Read())
					{
						lstAuditoriaItem.Add(new AuditoriaItem()
						{
							id = Convert.ToInt32(reader["id"]),
							id_grupo = Convert.ToInt32(reader["id_mcf_transparencia_grupo"]),
							informacao_auditada = reader["informacao_auditada"].ToString(),
							dispositivo_legal = reader["dispositivo_legal"].ToString()
						});
					}
				}

				foreach (var grupo in lstAuditoriaGrupo)
				{
					var itens = lstAuditoriaItem.FindAll(o => o.id_grupo == grupo.id);

					grupo.itens.AddRange(itens);
				}

				return new
				{
					grupos = lstAuditoriaGrupo,
					signatarios = new List<AuditoriaSignatario>() { new AuditoriaSignatario() }
				};
			}
		}

		[HttpPost]
		[Route("")]
		public HttpResponseMessage Salvar(Auditoria auditoria)
		{
			int? userId;
			try
			{
				var identity = (ClaimsIdentity)User.Identity;
				userId = Convert.ToInt32(identity.FindFirst("UserId").Value);
			}
			catch (Exception)
			{
				userId = null;
			}

			using (Banco banco = new Banco())
			{
				banco.BeginTransaction();

				banco.AddParameter("codigo", Guid.NewGuid().ToString());
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
				{
					foreach (var item in grupo.itens)
					{
						banco.AddParameter("id_mcf_auditoria", id_mcf_auditoria);
						banco.AddParameter("situacao", item.situacao);
						banco.AddParameter("indicio_de_prova", item.indicio_de_prova);

						banco.ExecuteScalar(
							@"INSERT INTO mcf_auditoria_item (
								id_mcf_auditoria, situacao, indicio_de_prova
							) VALUES (
								@id_mcf_auditoria, @situacao, @indicio_de_prova
							);");
					}
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

					var id_signatario = banco.ExecuteScalar(
						@"INSERT INTO mcf_signatario (
								nome, cpf, rg, nacionalidade, estado_civil, profissao, cep, endereco, bairro, cidade, id_estado
							) VALUES (
								@nome, @cpf, @rg, @nacionalidade, @estado_civil, @profissao, @cep, @endereco, @bairro, @cidade, @id_estado
							);
							SELECT LAST_INSERT_ID();");

					var id_mcf_signatario = Convert.ToInt32(id_signatario);

					banco.AddParameter("id_mcf_auditoria", id_mcf_auditoria);
					banco.AddParameter("id_mcf_signatario", id_mcf_signatario);

					var id_sig = banco.ExecuteScalar(
						@"INSERT INTO mcf_auditoria_signatario (
								id_mcf_auditoria, id_mcf_signatario
							) VALUES (
								@id_mcf_auditoria, @id_mcf_signatario
							);");
				}

				banco.CommitTransaction();
			}

			return GerarDenuncia(auditoria);
		}

		public HttpResponseMessage GerarDenuncia(Auditoria auditoria)
		{
			var newDocument = HostingEnvironment.MapPath(string.Concat("~/temp/", Guid.NewGuid().ToString(), ".docx"));
			var doc = new DenunciaWord();
			doc.CreatePackage(newDocument);

			//using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(newDocument, true))
			//{
			//	string docText = null;
			//	using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
			//	{
			//		docText = sr.ReadToEnd();
			//	}

			//	docText = docText.Replace("#CIDADE_AUDITADA#", auditoria.cidade);
			//	docText = docText.Replace("#DATA_EXTENSO#", DateTime.Now.ToLongDateString());

			//	var auditor = auditoria.signatarios[0];
			//	docText = docText.Replace("#DADOS_DO_AUDITOR#",
			//		string.Concat(auditor.nacionalidade, ", ",
			//		auditor.estado_civil, ", ",
			//		auditor.profissao, ", ",
			//		"portador da carteira de identidade nº ", auditor.rg, ", ",
			//		"inscrito no CPF nº ", auditor.cpf, ", ",
			//		"residente e domiciliado na ", auditor.endereco, ", ",
			//		"CEP ", auditor.cep, ", ",
			//		"Bairro ", auditor.bairro, ", ",
			//		"Cidade de ", auditor.cidade, ", ",
			//		auditor.estado)); //TODO: converter para texto

			//	docText = docText.Replace("#CIDADE_AUDITOR#", auditor.cidade);
			//	docText = docText.Replace("#NOME_AUDITOR#", auditor.nome_completo);
			//	docText = docText.Replace("#CPF_AUDITOR#", auditor.cpf);

			//	var grupos = auditoria.grupos.Where(g => g.itens.Any(i => i.situacao != "1" && i.situacao != "4"));
			//	foreach (var grupo in auditoria.grupos)
			//	{
			//		docText = docText.Replace("#GRUPO#", xmlGrupo);
			//		docText = docText.Replace("#GRUPO_AUDITADO#", grupo.nome);

			//		var itens = grupo.itens.Where(i => i.situacao != "1" && i.situacao != "4");
			//		foreach (var item in itens)
			//		{
			//			string situacao = string.Empty;
			//			switch (item.situacao)
			//			{
			//				case "2": situacao = "Atende parcialmente"; break;
			//				case "3": situacao = "Informação não encontrada"; break;
			//			}

			//			docText = docText.Replace("#ITEM#", xmlItem);

			//			docText = docText.Replace("#SITUACAO_DA_INFORMACAO#", situacao);
			//			docText = docText.Replace("#DESCRICAO_DA_INFORMACAO#", item.informacao_auditada);
			//			docText = docText.Replace("#INDICIO_DE_PROVA#", item.indicio_de_prova);
			//			docText = docText.Replace("#DISPOSITIVO_LEGAL#", item.dispositivo_legal);
			//		}
			//	}

			//	using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
			//	{
			//		sw.Write(docText);
			//	}
			//}

			//HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
			//var stream = new FileStream(newDocument, FileMode.Open, FileAccess.Read);
			//result.Content = new StreamContent(stream);
			//result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			//return result;

			return null;
		}
	}
}
