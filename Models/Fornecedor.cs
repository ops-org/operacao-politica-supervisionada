using System;
using System.Collections.Generic;

namespace OPS.Models
{
	public class Fornecedor
	{
		public string CnpjCpf { get; set; }
		public string DataAbertura { get; set; }
		public string RazaoSocial { get; set; }
		public string NomeFantasia { get; set; }
		public string AtividadePrincipal { get; set; }
		public string NaturezaJuridica { get; set; }
		public string Logradouro { get; set; }
		public string Numero { get; set; }
		public string Complemento { get; set; }
		public string Cep { get; set; }
		public string Bairro { get; set; }
		public string Cidade { get; set; }
		public string Uf { get; set; }
		public string Situacao { get; set; }
		public string DataSituacao { get; set; }
		public string MotivoSituacao { get; set; }
		public string SituacaoEspecial { get; set; }
		public string DataSituacaoEspecial { get; set; }

		public string Email { get; set; }
		public string Telefone { get; set; }
		public string EnteFederativoResponsavel { get; set; }
		//public string AtividadeSecundaria { get; set; }
		public bool Doador { get; set; }
		public string UsuarioInclusao { get; set; }
		public string DataInclusao { get; set; }
		public int Matriz { get; internal set; }
		public string CapitalSocial { get; set; }

		//TODO: Refatorar
		//public string AtividadeSecundaria01 { get; set; }
		//public string AtividadeSecundaria02 { get; set; }
		//public string AtividadeSecundaria03 { get; set; }
		//public string AtividadeSecundaria04 { get; set; }
		//public string AtividadeSecundaria05 { get; set; }
		//public string AtividadeSecundaria06 { get; set; }
		//public string AtividadeSecundaria07 { get; set; }
		//public string AtividadeSecundaria08 { get; set; }
		//public string AtividadeSecundaria09 { get; set; }
		//public string AtividadeSecundaria10 { get; set; }
		//public string AtividadeSecundaria11 { get; set; }
		//public string AtividadeSecundaria12 { get; set; }
		//public string AtividadeSecundaria13 { get; set; }
		//public string AtividadeSecundaria14 { get; set; }
		//public string AtividadeSecundaria15 { get; set; }
		//public string AtividadeSecundaria16 { get; set; }
		//public string AtividadeSecundaria17 { get; set; }
		//public string AtividadeSecundaria18 { get; set; }
		//public string AtividadeSecundaria19 { get; set; }
		//public string AtividadeSecundaria20 { get; set; }

		public string[] AtividadeSecundaria { get; set; }

		public List<FornecedorQuadroSocietario> lstFornecedorQuadroSocietario { get; internal set; }
	}
}