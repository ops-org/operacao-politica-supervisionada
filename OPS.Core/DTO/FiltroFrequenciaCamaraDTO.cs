using System.Collections.Generic;

namespace OPS.Core.DTO
{
	public class FiltroFrequenciaCamaraDTO
	{
		//public string sorting { get; set; }
		//public int count { get; set; }
		//public int page { get; set; }

		public int draw { get; set; }
		public int start { get; set; }
		public int length { get; set; }
		public Dictionary<string, object> order { get; set; }

		//public string IdParlamentar { get; set; }

		//public string NomeParlamentar { get; set; }

		//public string Despesa { get; set; }

		//public string Uf { get; set; }

		//public string Partido { get; set; }

		//public string Fornecedor { get; set; }

		//public string Periodo { get; set; }

		//public string Documento { get; set; }

		//public eAgrupamentoAuditoria Agrupamento { get; set; }

		public FiltroFrequenciaCamaraDTO()
		{
			this.start = 0;
			this.length = 500;
			//this.Agrupamento = eAgrupamentoAuditoria.Parlamentar;
		}
	}
}
