using OPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OPS.Core
{
	public class FormatarDados
	{
		public Fornecedor MontarObjFornecedor(string cnpj, string responseFromServer)
		{
			Fornecedor fornecedor = new Fornecedor();

			if (responseFromServer.IndexOf("NOME EMPRESARIAL") > 0)
			{
				string textoHTML = Regex.Replace(responseFromServer, @"<[^>]*>", string.Empty);
				textoHTML = textoHTML.Substring(textoHTML.IndexOf("NÚMERO DE INSCRIÇÃO"));
				textoHTML = textoHTML.Substring(0, textoHTML.IndexOf("Aprovado pela Instrução Normativa")).Replace("NÚMERO DE INSCRIÇÃO", "").Trim();
				textoHTML = Regex.Replace(textoHTML, "&nbsp;", string.Empty).Trim();
				fornecedor.CnpjCpf = cnpj; //textoHTML.Substring(0, textoHTML.IndexOf("\r\n"));

				textoHTML = textoHTML.Replace(fornecedor.CnpjCpf, "");
				fornecedor.Matriz = (textoHTML.Substring(0, textoHTML.IndexOf("COMPROVANTE")).Trim().Equals("MATRIZ")) ? 1 : 0;

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("DATA DE ABERTURA")).Replace("DATA DE ABERTURA", "").Trim();
				fornecedor.DataAbertura = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("NOME EMPRESARIAL")).Replace("NOME EMPRESARIAL", "").Trim();
				fornecedor.RazaoSocial = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("(NOME DE FANTASIA)")).Replace("(NOME DE FANTASIA)", "").Trim();
				fornecedor.NomeFantasia = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.NomeFantasia.Substring(0, 1) == "*")
					fornecedor.NomeFantasia = fornecedor.RazaoSocial;

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("CÓDIGO E DESCRIÇÃO DA ATIVIDADE ECONÔMICA PRINCIPAL")).Replace("CÓDIGO E DESCRIÇÃO DA ATIVIDADE ECONÔMICA PRINCIPAL", "").Trim();
				fornecedor.AtividadePrincipal = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.AtividadePrincipal.Substring(0, 1) == "*")
					fornecedor.AtividadePrincipal = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("CÓDIGO E DESCRIÇÃO DAS ATIVIDADES ECONÔMICAS SECUNDÁRIAS")).Replace("CÓDIGO E DESCRIÇÃO DAS ATIVIDADES ECONÔMICAS SECUNDÁRIAS", "").Trim();
				var sAtividadeSecundaria = textoHTML.Substring(0, textoHTML.IndexOf("CÓDIGO E DESCRIÇÃO DA NATUREZA JURÍDICA")).Trim();
				if (sAtividadeSecundaria.Equals("Não informada"))
					fornecedor.AtividadeSecundaria = new string[0];
				else
				{
					sAtividadeSecundaria = sAtividadeSecundaria.Replace("  ", "").Replace("\t", "").Replace("\r\n\r\n\r\n\r\n", "");
					fornecedor.AtividadeSecundaria = sAtividadeSecundaria.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				}

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("CÓDIGO E DESCRIÇÃO DA NATUREZA JURÍDICA")).Replace("CÓDIGO E DESCRIÇÃO DA NATUREZA JURÍDICA", "").Trim();
				fornecedor.NaturezaJuridica = textoHTML.Substring(0, textoHTML.IndexOf("PORTE DA EMPRESA")).Replace("PORTE DA EMPRESA", "").Trim();
				if (fornecedor.NaturezaJuridica.Equals("LOGRADOURO"))
					fornecedor.NaturezaJuridica = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("LOGRADOURO")).Replace("LOGRADOURO", "").Trim();
				fornecedor.Logradouro = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Logradouro.Equals("NÚMERO"))
					fornecedor.Logradouro = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("NÚMERO")).Replace("NÚMERO", "").Trim();
				fornecedor.Numero = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Numero.Equals("COMPLEMENTO"))
					fornecedor.Numero = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("COMPLEMENTO")).Replace("COMPLEMENTO", "").Trim();
				fornecedor.Complemento = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Complemento.Equals("CEP"))
					fornecedor.Complemento = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("CEP")).Replace("CEP", "").Trim();
				fornecedor.Cep = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim().Replace(".", "");
				if (fornecedor.Cep.Equals("DISTRITO"))
					fornecedor.Cep = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("DISTRITO")).Replace("DISTRITO", "").Trim();
				fornecedor.Bairro = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Bairro.Equals("MUNICÍPIO"))
					fornecedor.Bairro = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("MUNICÍPIO")).Replace("MUNICÍPIO", "").Trim();
				fornecedor.Cidade = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Cidade.Equals("UF"))
					fornecedor.Cidade = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("UF")).Replace("UF", "").Trim();
				fornecedor.Uf = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Uf.Equals("ENDEREÇO ELETRÔNICO"))
					fornecedor.Uf = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("ENDEREÇO ELETRÔNICO")).Replace("ENDEREÇO ELETRÔNICO", "").Trim();
				fornecedor.Email = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Email.Equals("TELEFONE"))
					fornecedor.Email = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("TELEFONE")).Replace("TELEFONE", "").Trim();
				fornecedor.Telefone = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Telefone.Equals("ENTE FEDERATIVO RESPONSÁVEL(EFR)"))
					fornecedor.Telefone = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("ENTE FEDERATIVO RESPONSÁVEL (EFR)")).Replace("ENTE FEDERATIVO RESPONSÁVEL (EFR)", "").Trim();
				fornecedor.EnteFederativoResponsavel = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.EnteFederativoResponsavel.Substring(0, 1) == "*")
					fornecedor.EnteFederativoResponsavel = "";

				textoHTML = ReplaceFirst(textoHTML.Substring(textoHTML.IndexOf("SITUAÇÃO CADASTRAL")), "SITUAÇÃO CADASTRAL", "").Trim();
				fornecedor.Situacao = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.Situacao.Equals("DATA DA SITUAÇÃO CADASTRAL"))
					fornecedor.Situacao = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("DATA DA SITUAÇÃO CADASTRAL")).Replace("DATA DA SITUAÇÃO CADASTRAL", "").Trim();
				fornecedor.DataSituacao = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.DataSituacao.Equals("MOTIVO DE SITUAÇÃO CADASTRAL"))
					fornecedor.DataSituacao = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("MOTIVO DE SITUAÇÃO CADASTRAL")).Replace("MOTIVO DE SITUAÇÃO CADASTRAL", "").Trim();
				fornecedor.MotivoSituacao = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.MotivoSituacao.Equals("SITUAÇÃO ESPECIAL"))
					fornecedor.MotivoSituacao = "";

				textoHTML = ReplaceFirst(textoHTML.Substring(textoHTML.IndexOf("SITUAÇÃO ESPECIAL")), "SITUAÇÃO ESPECIAL", "").Trim();
				fornecedor.SituacaoEspecial = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (fornecedor.SituacaoEspecial.Substring(0, 1) == "*")
					fornecedor.SituacaoEspecial = "";

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("DATA DA SITUAÇÃO ESPECIAL")).Replace("DATA DA SITUAÇÃO ESPECIAL", "").Trim();
				fornecedor.DataSituacaoEspecial = textoHTML;
				if (fornecedor.DataSituacaoEspecial.Substring(0, 1) == "*")
					fornecedor.DataSituacaoEspecial = "";

				return fornecedor;

			}
			else if (responseFromServer.IndexOf("Verifique se o mesmo foi digitado corretamente") > 0)
			{
				throw new System.Exception("CNPJ não localizado junto a receita federal.");
			}
			else if (responseFromServer.IndexOf("Digite os caracteres acima:") > 0)
			{
				throw new System.Exception("Capcha incorreto.");
			}
			return null;
		}

		private string ReplaceFirst(string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public void MontarObjFornecedorQuadroSocietario(Fornecedor fornecedor, string responseFromServer)
		{
			fornecedor.lstFornecedorQuadroSocietario = new List<FornecedorQuadroSocietario>();
			if (responseFromServer.IndexOf("CAPITAL SOCIAL:") > 0)
			{
				string textoHTML = Regex.Replace(responseFromServer, @"<[^>]*>", string.Empty);

				textoHTML = textoHTML.Substring(textoHTML.IndexOf("CAPITAL SOCIAL:")).Replace("CAPITAL SOCIAL:", "").Trim();
				fornecedor.CapitalSocial = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
				if (!fornecedor.CapitalSocial.Contains("R$"))
				{
					fornecedor.CapitalSocial = "";
				}

				while (textoHTML.Contains("Nome/Nome Empresarial:"))
				{
					var fornecedorQuadroSocietario = new FornecedorQuadroSocietario();

					textoHTML = ReplaceFirst(textoHTML.Substring(textoHTML.IndexOf("Nome/Nome Empresarial:")), "Nome/Nome Empresarial:", "").Trim();
					fornecedorQuadroSocietario.Nome = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();

					textoHTML = ReplaceFirst(textoHTML.Substring(textoHTML.IndexOf("Qualificação:")), "Qualificação:", "").Trim();
					fornecedorQuadroSocietario.Qualificacao = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();

					//Valor Opcional. ex: 07436265000186
					if (textoHTML.IndexOf("Qualif. Rep. Legal:") > 0 &&
						(textoHTML.IndexOf("Nome/Nome Empresarial:") < 0 || textoHTML.IndexOf("Qualif. Rep. Legal:") < textoHTML.IndexOf("Nome/Nome Empresarial:")))
					{
						textoHTML = ReplaceFirst(textoHTML.Substring(textoHTML.IndexOf("Qualif. Rep. Legal:")), "Qualif. Rep. Legal:", "").Trim();
						fornecedorQuadroSocietario.QualificacaoRepresentanteLegal = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();
					}

					//Valor Opcional. ex: 07436265000186
					if (textoHTML.IndexOf("Nome do Repres. Legal:") > 0 &&
						(textoHTML.IndexOf("Nome/Nome Empresarial:") < 0 || textoHTML.IndexOf("Nome do Repres. Legal:") < textoHTML.IndexOf("Nome/Nome Empresarial:")))
					{
						textoHTML = ReplaceFirst(textoHTML.Substring(textoHTML.IndexOf("Nome do Repres. Legal:")), "Nome do Repres. Legal:", "").Trim();
						fornecedorQuadroSocietario.NomeRepresentanteLegal = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Trim();

					}

					fornecedor.lstFornecedorQuadroSocietario.Add(fornecedorQuadroSocietario);
				}
			}
		}
	}
}