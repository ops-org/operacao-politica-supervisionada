using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPS.Models
{
	public class AuditoriaItem
	{
		public int id { get; set; }
		public int id_grupo { get; set; }
		public string informacao_auditada { get; set; }
		public string dispositivo_legal { get; set; }
		public string situacao { get; set; }
		public string indicio_de_prova { get; set; }
	}

	public class AuditoriaGrupo
	{
		public int id { get; set; }
		public string nome { get; set; }
		public List<AuditoriaItem> itens { get; set; }
	}

	public class AuditoriaSignatario
	{
		public string nome_completo { get; set; }
		public string rg { get; set; }
		public string cpf { get; set; }
		public string profissao { get; set; }
		public string estado_civil { get; set; }
		public string nacionalidade { get; set; }
		public string endereco { get; set; }
		public string cep { get; set; }
		public string bairro { get; set; }
		public string cidade { get; set; }
		public string estado { get; set; }
		public string email { get; set; }
	}

	public class Auditoria
	{
		public string codigo { get; set; }
		public List<AuditoriaGrupo> grupos { get; set; }
		public List<AuditoriaSignatario> signatarios { get; set; }
		public string estado { get; set; }
		public string cidade { get; set; }
		public string link_portal { get; set; }
	}
}