using System.Globalization;
using Castle.Core.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    [Flags]
    public enum StringConversionOptions
    {
        None = 0,
        Cleaning = 1,
        TitleCase = 2,
        LowerCase = 4
    }

    public class StringConverterCustom : StringConverter
    {
        private readonly StringConversionOptions _options;

        public StringConverterCustom() : this(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)
        {
        }

        public StringConverterCustom(StringConversionOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Converts the string based on the specified options.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
        /// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
        /// <returns>The converted string created from the input string.</returns>
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string result = text;

            // Apply cleaning if enabled
            if (_options.HasFlag(StringConversionOptions.Cleaning))
            {
                if (text == "#NE" || text == "#NULO" || text == "#NULO#" || text == "NÃO DIVULGÁVEL")
                {
                    return null;
                }
            }

            // Apply title case if enabled
            if (_options.HasFlag(StringConversionOptions.TitleCase))
            {
                result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
                result = result.Replace(" De ", " de ").Replace(" Da ", " da ").Replace(" E ", " e ").Replace("(A)", "(a)");
            }
            // Apply lower case if enabled (and title case is not enabled)
            else if (_options.HasFlag(StringConversionOptions.LowerCase))
            {
                result = text.ToLower();
            }

            return result;
        }
    }

    public class NumberWithCleanerConverter : StringConverter
    {
        /// <summary>
        /// Converts the string to a valid number or null.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
        /// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
        /// <returns>The number or null created from the input string.</returns>
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (text.All(char.IsDigit))
                return text;

            if (!(text == "-1" || text == "-2" || text == "-3" || text == "-4"))
                Console.WriteLine("Revisar Valor Ignorado: " + text);

            return null;
        }
    }
}
