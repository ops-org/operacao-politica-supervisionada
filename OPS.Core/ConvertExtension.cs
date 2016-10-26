using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;

namespace OPS.Core
{
	/// <summary>
	/// Classe de conversao de tipo
	/// </summary>
	public static class TypeConversion
	{
		/// <summary>
		/// Converte para booleano, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static bool ToBool(this IConvertible value, bool other = false)
		{
			try
			{
				return Convert.ToBoolean(value);
			}
			catch (Exception)
			{
				return other;
			}
		}

		/// <summary>
		/// Converte para booleano, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static bool ToBool(object value, bool other = false)
		{
			try
			{
				return Convert.ToBoolean(value);
			}
			catch (Exception)
			{
				return other;
			}
		}

		/// <summary>
		/// Converte para long, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static long ToLong(this IConvertible value, long other = 0)
		{
			try
			{
				if (value is long)
					return (long)value;

				return (long)Math.Truncate(value.ToDecimal());
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para long, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static long ToLong(object value, long other = 0)
		{
			try
			{
				if (value is long)
					return (long)value;

				return (long)Math.Truncate(ToDecimal(value));
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para long, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static long? ToLongNullable(this IConvertible value)
		{
			try
			{
				if (value is long)
					return (long)value;

				var newValue = value.ToDecimalNullable();
				if (newValue == null)
					return null;
				return (long)Math.Truncate(Convert.ToDecimal(newValue));
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Converte para long, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static long? ToLongNullable(object value)
		{
			try
			{
				if (value is long)
					return (long)value;

				return (long)Math.Truncate(ToDecimal(value));
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Converte para int, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static int ToInt(this IConvertible value, int other = 0)
		{
			try
			{
				if (value is int)
					return (int)value;

				return (int)Math.Truncate(value.ToDecimal());
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para int, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static int ToInt(object value, int other = 0)
		{
			try
			{
				if (value is int)
					return (int)value;

				return (int)Math.Truncate(ToDecimal(value));
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para int, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static int? ToIntNullable(this IConvertible value)
		{
			try
			{
				if (value is int)
					return (int)value;

				var newValue = value.ToDecimalNullable();
				if (newValue == null)
					return null;
				return (int)Math.Truncate(Convert.ToDecimal(newValue));
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Converte para int, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static int? ToIntNullable(object value)
		{
			try
			{
				if (value is int)
					return (int)value;

				return (int)Math.Truncate(ToDecimal(value));
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Verifica se a data é valida. Sendo válida retorna no formato "dd/MM/yyyy" se não retorna vazio
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToDateString(this DateTime value)
		{
			var valString = value.ToString("dd/MM/yyyy");
			return valString == "01/01/0001" || valString == "01/01/1800" || valString == "01/01/1900" || valString == "30/12/1899" || valString == "18/12/1899" ? string.Empty : valString;
		}

		/// <summary>
		/// Verifica se a data é valida. Sendo válida retorna no formato "dd/MM/yyyy HH:mm" se não retorna vazio
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToDateTimeString(this DateTime value)
		{
			var valString = value.ToString("dd/MM/yyyy HH:mm");
			return valString == "01/01/0001 00:00" || valString == "01/01/1800 00:00" || valString == "01/01/1900 00:00" || valString == "30/12/1899 00:00" || valString == "18/12/1899 00:00" ? string.Empty : value.ToString("dd/MM/yyyy HH:mm");
		}

		/// <summary>
		/// Retorna no formato "HH:mm"
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToTimeString(this DateTime value)
		{
			return value.ToString("HH:mm");
		}

		/// <summary>
		/// Converte para decimal, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static decimal ToDecimal(this IConvertible value, decimal other = 0)
		{
			try
			{
				if (value is decimal)
					return (decimal)value;

				var valueString = value.ToString(CultureInfo.InvariantCulture);
				if (valueString.Contains(","))
					valueString = valueString.Replace(".", "").Replace(",", ".");

				decimal temp;
				if (decimal.TryParse(valueString, NumberStyles.Number, NumberFormatInvariant, out temp))
					return temp;
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para decimal, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static decimal ToDecimal(object value, decimal other = 0)
		{
			try
			{
				if (value is decimal)
					return (decimal)value;

				var valueString = value.ToString();
				if (valueString.Contains(","))
					valueString = valueString.Replace(".", "").Replace(",", ".");

				decimal temp;
				if (decimal.TryParse(valueString, NumberStyles.Number, NumberFormatInvariant, out temp))
					return temp;
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para decimal, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static decimal? ToDecimalNullable(this IConvertible value)
		{
			try
			{
				if (value is decimal)
					return (decimal)value;

				var valueString = value.ToString(CultureInfo.InvariantCulture);
				if (valueString.Contains(","))
					valueString = valueString.Replace(".", "").Replace(",", ".");

				decimal temp;
				if (decimal.TryParse(valueString, NumberStyles.Number, NumberFormatInvariant, out temp))
					return temp;
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Converte para decimal, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static decimal? ToDecimalNullable(object value)
		{
			try
			{
				if (value is decimal)
					return (decimal)value;

				var valueString = value.ToString();
				if (valueString.Contains(","))
					valueString = valueString.Replace(".", "").Replace(",", ".");

				decimal temp;
				if (decimal.TryParse(valueString, NumberStyles.Number, NumberFormatInvariant, out temp))
					return temp;
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Converte para DateTime, caso aconteça erro, retornar DateTime.MinValue
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor Convertido ou DateTime.MinValue</returns>
		public static DateTime ToDate(this IConvertible value)
		{
			return value.ToDate(DateTime.MinValue);
		}

		/// <summary>
		/// Converte para DateTime, caso aconteça erro, retornar o parametro other
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other">Valor caso aconteça erro</param>
		/// <returns>Valor convertido ou o valor other</returns>
		public static DateTime ToDate(this IConvertible value, DateTime other)
		{
			try
			{
				if (value is string && string.IsNullOrEmpty(value.ToString())) return other;
				//var cultureinfo =  new CultureInfo("pt-br"); 

				DateTime temp;
				IFormatProvider format = new CultureInfo("pt-br", true);
				if (DateTime.TryParse(value.ToString(CultureInfo.InvariantCulture), format, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out temp))
					if (!string.IsNullOrEmpty(temp.ToDateTimeString()))
						return temp;
			}
			catch { }
			return other;
		}

		/// <summary>
		/// Converte para DateTime, caso aconteça erro, retornar null
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Valor convertido ou o valor null</returns>
		public static DateTime? ToDateNullable(this IConvertible value)
		{
			try
			{
				DateTime temp;
				if (DateTime.TryParse(value.ToString(CultureInfo.InvariantCulture), DateTimeFormatInvariant, DateTimeStyles.NoCurrentDateDefault, out temp))
					if (!string.IsNullOrEmpty(temp.ToDateString()))
						return temp;
			}
			catch { }
			return null;
		}

		/// <summary>
		/// Verifica Vazio, Null, Apenas Espaços e espaço HTML
		/// </summary>
		/// <param name="value">Valor para comparar</param>
		/// <returns>retorna true ou false</returns>
		public static bool IsNullOrEmpty(this object value)
		{
			return string.IsNullOrEmpty(value.ToString().Trim()) || value.ToString().Equals("&nbsp;");
		}

		/// <summary>
		/// Verifica Vazio, Null, Apenas Espaços, espaço HTML e IsDBNull
		/// </summary>
		/// <param name="value">Valor para comparar</param>
		/// <returns>retorna true ou false</returns>
		public static bool IsNullOrEmptyOrIsDBNull(this object value)
		{
			return Convert.IsDBNull(value) || value.IsNullOrEmpty();
		}

		/// <summary>
		/// Verifica Vazio, Null, Apenas Espaços, espaço HTML, Zero e Mascaras("__/__/_____" e "__:__")
		/// </summary>
		/// <param name="value">Valor para comparar</param>
		/// <returns>retorna true ou false</returns>
		public static bool IsNullEmptyOrZero(this object value)
		{
			return value.IsNullOrEmpty() || value.Equals("0") || value.Equals("&nbsp;") || value.Equals("__/__/____") || value.Equals("__:__");
		}

		/// <summary>
		/// Converter para o tipo do parâmetro "T"
		/// </summary>
		/// <typeparam name="T">Tipo para Conversão</typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		private static T To<T>(this IConvertible obj)
		{
			return (T)Convert.ChangeType(obj, typeof(T));
		}

		/// <summary>
		/// Converte para o tipo T ou retorna o valor default do tipo T.
		/// </summary>
		/// <typeparam name="T">Tipo para Conversão</typeparam>
		/// <param name="obj"></param>
		/// <returns>Retornar o valor Convertido ou Default</returns>
		private static T ToOrDefault<T>(this IConvertible obj)
		{
			try
			{
				return To<T>(obj);
			}
			catch
			{
				return default(T);
			}
		}

		/// <summary>
		/// Converte para o tipo T ou retornar o valor do other
		/// </summary>
		/// <typeparam name="T">Tipo para Conversão</typeparam>
		/// <param name="obj"></param>
		/// <param name="other">Valor que retornar se nao converter</param>
		/// <returns>Retornar o valor convertido ou o valor passado</returns>
		private static T ToOrOther<T>(this IConvertible obj, T other)
		{
			try
			{
				return To<T>(obj);
			}
			catch
			{
				return other;
			}
		}

		/// <summary>
		/// Verifica se o inicio e fim estao entre o valor
		/// </summary>
		/// <typeparam name="T">Tipo de Valor</typeparam>
		/// <param name="item"></param>
		/// <param name="inicio">Valor Inicial</param>
		/// <param name="fim">Valor Final</param>
		/// <returns></returns>
		public static bool IsBetween<T>(this T item, T inicio, T fim)
		{
			return Comparer<T>.Default.Compare(item, inicio) >= 0
				&& Comparer<T>.Default.Compare(item, fim) <= 0;
		}

		private static NumberFormatInfo NumberFormatInvariant
		{
			get { return CultureInfo.InvariantCulture.NumberFormat; }//GetCultureInfo("pt-BR")
		}

		private static DateTimeFormatInfo DateTimeFormatInvariant
		{
			get { return CultureInfo.InvariantCulture.DateTimeFormat; }
		}
	}
}
