using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPS.Core
{
	public class FiltroDropDownDTO
	{
		/// <summary>
		/// Filtro digitado pelo usuario
		/// </summary>
		public string q { get; set; }

		/// <summary>
		/// Pagina pesquisada
		/// </summary>
		public int? page { get; set; }

		public readonly int count = 30;
	}
}