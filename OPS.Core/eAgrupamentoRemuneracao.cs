using System;
using System.Collections.Generic;
using System.Web;

namespace OPS.Core
{
	public enum EnumAgrupamentoRemuneracao
	{
		/// <summary>
		/// Parlamentar (Deputado, Senador)
		/// </summary>
		Lotacao = 1,

		/// <summary>
		/// Tipo da Despeza
		/// </summary>
		Cargo = 2,

		/// <summary>
		/// Fornecedor
		/// </summary>
		Categoria = 3,

		/// <summary>
		/// Partido do Parlamentar
		/// </summary>
		Vinculo = 4,

		/// <summary>
		/// Estado do Parlamentar
		/// </summary>
		Ano = 5,

		/// <summary>
		/// Nota Fiscal ou Recibo
		/// </summary>
		AnoMes = 6
	}
}