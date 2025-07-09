using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OPS.Core.Utilities
{
    public class SqlTimestamp
    {
        [JsonPropertyName("@class")]
        public string Class { get; set; }

        [JsonPropertyName("$")]
        public string Timestamp { get; set; }
    }

    public class SqlTimestampConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var sqlTimestamp = JsonSerializer.Deserialize<SqlTimestamp>(ref reader, options);
                return DateTime.Parse(sqlTimestamp.Timestamp);
            }

            throw new JsonException("Invalid JSON format for SQL timestamp.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var sqlTimestamp = new SqlTimestamp
            {
                Class = "sql-timestamp",
                Timestamp = value.ToString("yyyy-MM-dd")
            };

            JsonSerializer.Serialize(writer, sqlTimestamp, options);
        }
    }
}
