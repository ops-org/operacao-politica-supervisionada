using System.Collections.Generic;

namespace OPS.Core
{
	public class FiltroParlamentarDTO
	{
		public string filter { get; set; }
		public string sorting { get; set; }
		public int count { get; set; }
		public int page { get; set; }

		public string IdParlamentar { get; set; }

		public string SgUF { get; set; }

		public string SgPartido { get; set; }

		public string Periodo { get; set; }

		public string CnpjCpf { get; set; }

		public FiltroParlamentarDTO()
		{
			count = 1;
			page = 1;
		}
	}
}
