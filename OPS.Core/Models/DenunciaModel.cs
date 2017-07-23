using System.Collections.Generic;

namespace OPS.Core.Models
{
	public class DenunciaModel
	{
		public string id_denuncia { get; set; }
		public int codigo { get; set; }
		public string cnpj_cpf { get; set; }
		public int id_fornecedor { get; set; }
		public string nome_fornecedor { get; set; }
		public string nome_usuario_denuncia { get; set; }
		public string data_denuncia { get; set; }
		public string texto { get; set; }
		public string anexo { get; set; }
		public string situacao { get; set; }
		public string situacao_descricao { get; set; }
		public string data_auditoria { get; set; }
		public string nome_usuario_auditoria { get; set; }
		public List<DenunciaAnexoModel> anexos { get; set; }
		public List<DenunciaMensagemModel> mensagens { get; set; }
	}

	public class DenunciaAnexoModel
	{
		public string nome_usuario { get; set; }
		public string data { get; set; }
		public string nome_arquivo { get; set; }
	}

	public class DenunciaMensagemModel
	{
		public string nome_usuario { get; set; }
		public string data { get; set; }
		public string texto { get; set; }
	}
}