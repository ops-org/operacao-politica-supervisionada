using System;

namespace OPS.Core
{
	public static class Utils
	{
		public static string FormataValor(object value)
		{
			if (!Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
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
			return "0,00";
		}

		public static string FormataData(object value)
		{
			if (!Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
			{
				try
				{
					return Convert.ToDateTime(value).ToString("dd/MM/yyyy");
				}
				catch (Exception)
				{ }
			}
			return "";
		}

		public static string FormataDataHora(object value)
		{
			if (!Convert.IsDBNull(value) && !string.IsNullOrEmpty(value.ToString()))
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

		public static object ParseDateTime(object d)
		{
			if (Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()) || d.ToString() == "0000-00-00 00:00:00" || d.ToString().StartsWith("*"))
			{
				return DBNull.Value;
			}
			else
			{
				try
				{
					return Convert.ToDateTime(d);
				}
				catch (Exception ex)
				{
					return DBNull.Value;
				}
			}
		}
	}
}
