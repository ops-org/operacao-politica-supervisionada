using System;
using System.Collections.Generic;
using System.Web;

namespace OPS.Core
{
	public enum eAgrupamentoAuditoria
	{
		/// <summary>
		/// Parlamentar (Deputado, Senador)
		/// </summary>
		Parlamentar = 1,

		/// <summary>
		/// Tipo da Despeza
		/// </summary>
		Despesa = 2,

		/// <summary>
		/// Fornecedor
		/// </summary>
		Fornecedor = 3,

		/// <summary>
		/// Partido do Parlamantar
		/// </summary>
		Partido = 4,

		/// <summary>
		/// Estado do Parlamentar
		/// </summary>
		Uf = 5,

		/// <summary>
		/// Nota Fiscal ou Recibo
		/// </summary>
		Documento = 6
	}
}