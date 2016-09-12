using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPS.Models
{
	public class Fornecedor
	{
		public String CnpjCpf { get; set; }
		public String DataAbertura { get; set; }
		public String RazaoSocial { get; set; }
		public String NomeFantasia { get; set; }
		public String AtividadePrincipal { get; set; }
		public String NaturezaJuridica { get; set; }
		public String Logradouro { get; set; }
		public String Numero { get; set; }
		public String Complemento { get; set; }
		public String Cep { get; set; }
		public String Bairro { get; set; }
		public String Cidade { get; set; }
		public String Uf { get; set; }
		public String Situacao { get; set; }
		public String DataSituacao { get; set; }
		public String MotivoSituacao { get; set; }
		public String SituacaoEspecial { get; set; }
		public String DataSituacaoEspecial { get; set; }

		public String Email { get; set; }
		public String Telefone { get; set; }
		public String EnteFederativoResponsavel { get; set; }
		public string AtividadeSecundaria { get; set; }
		public Boolean Doador { get; set; }
		public String UsuarioInclusao { get; set; }
		public string DataInclusao { get; set; }
		public int Matriz { get; internal set; }
		public String CapitalSocial { get; set; }

		//TODO: Refatorar
		public String AtividadeSecundaria01 { get; set; }
		public String AtividadeSecundaria02 { get; set; }
		public String AtividadeSecundaria03 { get; set; }
		public String AtividadeSecundaria04 { get; set; }
		public String AtividadeSecundaria05 { get; set; }
		public String AtividadeSecundaria06 { get; set; }
		public String AtividadeSecundaria07 { get; set; }
		public String AtividadeSecundaria08 { get; set; }
		public String AtividadeSecundaria09 { get; set; }
		public String AtividadeSecundaria10 { get; set; }
		public String AtividadeSecundaria11 { get; set; }
		public String AtividadeSecundaria12 { get; set; }
		public String AtividadeSecundaria13 { get; set; }
		public String AtividadeSecundaria14 { get; set; }
		public String AtividadeSecundaria15 { get; set; }
		public String AtividadeSecundaria16 { get; set; }
		public String AtividadeSecundaria17 { get; set; }
		public String AtividadeSecundaria18 { get; set; }
		public String AtividadeSecundaria19 { get; set; }
		public String AtividadeSecundaria20 { get; set; }

		public List<FornecedorQuadroSocietario> lstFornecedorQuadroSocietario { get; internal set; }
	}
}