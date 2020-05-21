using OPS.Core.DAO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OPS.Core.Models
{
	public class FormatarDados
	{
		private const string urlBaseReceitaFederal = "http://servicos.receita.fazenda.gov.br/Servicos/cnpjreva/";
		private const string paginaValidacao = "valida.asp";
		private const string paginaQuadroSocietario = "Cnpjreva_qsa.asp";

		public Fornecedor ObterDados(CookieContainer _cookies, string aCNPJ, string aCaptcha, bool bUsarSleap = false)
		{
			Random aleatorio = new Random();

			if (bUsarSleap)
				System.Threading.Thread.Sleep(aleatorio.Next(5000, 10000));

			string cnpj = new Regex(@"[^\d]").Replace(aCNPJ, string.Empty);

			var request = (HttpWebRequest)WebRequest.Create(urlBaseReceitaFederal + paginaValidacao);
			request.ProtocolVersion = HttpVersion.Version10;
			request.CookieContainer = _cookies;
            request.AllowAutoRedirect = true;
            request.Referer = "http://servicos.receita.fazenda.gov.br/Servicos/cnpjreva/Cnpjreva_Solicitacao_CS.asp";
            request.UserAgent = "Mozilla/4.0 (compatible; Synapse)";
            request.KeepAlive = true;
            request.Method = "POST";

			var postData = string.Empty;
			postData += "origem=comprovante";
			postData += "&cnpj=" + aCNPJ;
            postData += "&txtTexto_captcha_serpro_gov_br=" + aCaptcha;
            postData += "&search_type=cnpj";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteArray.Length;

			var dataStream = request.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();

			var response = request.GetResponse();
			var stHtml = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));
			var strHtmlFornecedor = stHtml.ReadToEnd();

			if (strHtmlFornecedor.Contains("Verifique se o mesmo foi digitado corretamente"))
				throw new BusinessException("O número do CNPJ não foi localizado na Receita Federal");

			if (strHtmlFornecedor.Contains("0,0"))
				throw new BusinessException("Os caracteres não conferem com a imagem");

			if (strHtmlFornecedor.Length > 0)
			{
				Fornecedor fornecedor = MontarObjFornecedor(cnpj, strHtmlFornecedor);
				if (fornecedor != null)
				{
					if (bUsarSleap)
						System.Threading.Thread.Sleep(aleatorio.Next(5000, 10000));

					var requestQAS = (HttpWebRequest)HttpWebRequest.Create(urlBaseReceitaFederal + paginaQuadroSocietario);
					requestQAS.CookieContainer = _cookies;
					requestQAS.Method = "GET";
					var res = (HttpWebResponse)requestQAS.GetResponse();
					StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));
					string strHtmlQuadroSocietario = sr.ReadToEnd();

					MontarObjFornecedorQuadroSocietario(fornecedor, strHtmlQuadroSocietario);

					//string UserName;
					//try
					//{
					//	UserName = HttpContext.Current.User.Identity.Name;
					//}
					//catch (Exception)
					//{
					//	UserName = "anonymous";
					//}

					//fornecedor.UsuarioInclusao = UserName;
					//fornecedor.DataInclusao = DateTime.Now.ToString();

					var fornecedorDao = new FornecedorDao();
					fornecedor.id = fornecedorDao.AtualizaDados(fornecedor);

					return fornecedor;
					// fornecedorDao.MarcaVisitado(fornecedor.CnpjCpf, UserName);
				}
			}

			return null;
		}

		public Fornecedor MontarObjFornecedor(string cnpj, string responseFromServer)
		{
			Fornecedor fornecedor = new Fornecedor();

			if (responseFromServer.IndexOf("NOME EMPRESARIAL") > 0)
			{
				string textoHTML = Regex.Replace(responseFromServer, @"<[^>]*>", string.Empty);
				textoHTML = textoHTML.Substring(textoHTML.IndexOf("NÚMERO DE INSCRIÇÃO"));
				textoHTML = textoHTML.Substring(0, textoHTML.IndexOf("Aprovado pela Instrução Normativa")).Replace("NÚMERO DE INSCRIÇÃO", "").Trim();
				textoHTML = Regex.Replace(textoHTML, "&nbsp;", string.Empty).Trim();
				fornecedor.CnpjCpf = cnpj;

				textoHTML = textoHTML.Replace(textoHTML.Substring(0, textoHTML.IndexOf("\r\n")), "");
				fornecedor.Tipo = textoHTML.Substring(0, textoHTML.IndexOf("COMPROVANTE")).Trim();

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
				fornecedor.NaturezaJuridica = textoHTML.Substring(0, textoHTML.IndexOf("LOGRADOURO")).Replace("LOGRADOURO", "").Trim();
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
				fornecedor.Complemento = Utils.SingleSpacedTrim(textoHTML.Substring(0, textoHTML.IndexOf("\r\n"))).Trim();
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
				fornecedor.DataSituacao = textoHTML.Substring(0, textoHTML.IndexOf("\r\n")).Replace("00:00:00", "").Trim();
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
				throw new BusinessException("CNPJ não localizado junto a receita federal.");
			}
			else if (responseFromServer.IndexOf("Digite os caracteres acima:") > 0)
			{
				throw new BusinessException("Capcha incorreto.");
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