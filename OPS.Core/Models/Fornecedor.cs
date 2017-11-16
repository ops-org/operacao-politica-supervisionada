using System.Collections.Generic;

namespace OPS.Core.Models
{
	public class Fornecedor
	{
		public int id { get; set; }
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
		public string Tipo { get; set; }
		public string CapitalSocial { get; set; }


		public string[] AtividadeSecundaria { get; set; }

		public List<FornecedorQuadroSocietario> lstFornecedorQuadroSocietario { get; internal set; }
	}
}