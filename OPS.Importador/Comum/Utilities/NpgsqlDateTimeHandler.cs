using System.Data;
using System.Globalization;
using Dapper;

namespace OPS.Importador.Comum.Utilities;

// This class was specific to MySQL and is no longer needed with PostgreSQL
// PostgreSQL handles DateTime properly without special handling
public class NpgsqlDateTimeHandler : SqlMapper.ITypeHandler
{
    public object Parse(Type destinationType, object value)
    {
        if (value == null || value is DBNull) return null;

        // PostgreSQL handles DateTime properly, so we can use direct conversion
        var destNullableType = Nullable.GetUnderlyingType(destinationType);
        var destType = destNullableType ?? destinationType;

        try
        {
            if (value is DateTime)
                return value;

            if (destType == typeof(DateTime))
            {
                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                    return parsed;
            }

            return Convert.ChangeType(value, destType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return destNullableType != null ? null : DateTime.MinValue;
        }
    }

    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.Value = value ?? DBNull.Value;
    }
}
