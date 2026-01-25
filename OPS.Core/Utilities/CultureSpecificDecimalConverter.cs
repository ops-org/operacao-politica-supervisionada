using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OPS.Core.Utilities;

public class CultureSpecificDecimalConverter : JsonConverter<decimal>
{
    private readonly CultureInfo _culture;

    public CultureSpecificDecimalConverter(CultureInfo culture)
    {
        _culture = culture;
    }

    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return Convert.ToDecimal(reader.GetString(), _culture);
        }
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}