using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Core
{
	public static class Utils
	{
		internal static string FormataValor(object v)
		{
			if (!Convert.IsDBNull(v))
			{
				try
				{
					return Convert.ToDecimal(v).ToString("#,##0.00");
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
