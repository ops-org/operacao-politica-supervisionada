using System;
using System.Collections.Generic;
using System.Web;

namespace OPS.Core
{
	public class FiltroUsuarioDTO
	{
		public string filter { get; set; }
		public string sorting { get; set; }
		public int count { get; set; }
		public int page { get; set; }

		public string uf { get; set; }

		public FiltroUsuarioDTO()
		{
			this.count = 1;
			this.page = 1;
		}
	}
}