﻿using System;
using System.Collections.Generic;
using System.Web;

namespace OPS.Core
{
	public enum EnumAgrupamentoAuditoria
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
		/// Partido do Parlamentar
		/// </summary>
		Partido = 4,

		/// <summary>
		/// Estado do Parlamentar
		/// </summary>
		Estado = 5,

		/// <summary>
		/// Nota Fiscal ou Recibo
		/// </summary>
		Documento = 6
	}
}