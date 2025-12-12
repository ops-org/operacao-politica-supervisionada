using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OPS.Importador.SenadoFederal.Entities;

public class Partido
{
    public string CodigoPartido { get; set; }
    public string Sigla { get; set; }
    public string Nome { get; set; }
    public string DataFiliacao { get; set; }
    public string DataDesfiliacao { get; set; }
}

public class Partidos
{
    [JsonConverter(typeof(SingleOrListConverter<Partido>))]
    public List<Partido> Partido { get; set; } = new List<Partido>();
}

public class SingleOrListConverter<T> : JsonConverter<List<T>>
{
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new List<T>();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // It's an array, deserialize as list
            var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
            return list ?? new List<T>();
        }
        else
        {
            // It's a single object, wrap in list
            var item = JsonSerializer.Deserialize<T>(ref reader, options);
            return item != null ? new List<T> { item } : new List<T>();
        }
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        if (value == null || value.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Count == 1)
        {
            // Write as single object
            JsonSerializer.Serialize(writer, value[0], options);
        }
        else
        {
            // Write as array
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
