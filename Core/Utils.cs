using System;

namespace OPS.Core
{
	public static class Utils
	{
		internal static string FormataValor(object value)
		{
			if (!Convert.IsDBNull(value))
			{
				try
				{
					return Convert.ToDecimal(value).ToString("#,##0.00");
				}
				catch (Exception)
				{
					throw;
				}
			}
			return "";
		}

		internal static string FormataData(object value)
		{
			if (!Convert.IsDBNull(value))
			{
				try
				{
					return Convert.ToDateTime(value).ToString("dd/MM/yyyy");
				}
				catch (Exception)
				{
					throw;
				}
			}
			return "";
		}

		internal static string FormataDataHora(object value)
		{
			if (!Convert.IsDBNull(value))
			{
				try
				{
					return Convert.ToDateTime(value).ToString("dd/MM/yyyy HH:mm");
				}
				catch (Exception)
				{
					throw;
				}
			}
			return "";
		}
	}
}
