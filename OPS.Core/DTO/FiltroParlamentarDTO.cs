namespace OPS.Core.DTO
{
	public class FiltroParlamentarDTO
	{
		//public string filter { get; set; }
		public string sorting { get; set; }
		public int count { get; set; }
		public int page { get; set; }

		public string IdParlamentar { get; set; }

		public string NomeParlamentar { get; set; }

		public string Despesa { get; set; }

		public string Uf { get; set; }

		public string Partido { get; set; }

		public string Fornecedor { get; set; }

		public string Periodo { get; set; }

		//public string Documento { get; set; }

		public eAgrupamentoAuditoria Agrupamento { get; set; }

		public FiltroParlamentarDTO()
		{
			this.count = 100;
			this.page = 1;
			this.Agrupamento = eAgrupamentoAuditoria.Parlamentar; 
		}
	}
}
