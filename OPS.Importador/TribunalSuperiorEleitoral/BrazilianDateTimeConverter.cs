using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public class BrazilianDateTimeConverter : DateTimeConverter
    {
        public BrazilianDateTimeConverter()
        {
            // Constructor that passes the input and output formats to base class
            // Input: dd/MM/yyyy (Brazilian format from TSE CSV)
            // Output: yyyy-MM-dd (ISO format for database)
        }

        /// <summary>
        /// Converts the string to an object.
        /// </summary>
        /// <param name="text">The string to convert to an object.</param>
        /// <param name="row">The <see cref="IReaderRow"/> for the current record.</param>
        /// <param name="memberMapData">The <see cref="MemberMapData"/> for the member being created.</param>
        /// <returns>The object created from the string.</returns>
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text == null)
            {
                return base.ConvertFromString(null, row, memberMapData);
            }

            var success = DateTime.TryParseExact(text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

            if (dateTime == DateTime.MinValue)
            {
                if (!string.IsNullOrEmpty(text))
                    Console.WriteLine($"Data invalida: {text}");

                return null;
            }

            return success ? dateTime.ToString("yyyy-MM-dd") : null;
        }
    }
}
