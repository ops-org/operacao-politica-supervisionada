using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dapper;

namespace OPS.Importador.Utilities;


public class MySqlConnectorDateTimeHandler : SqlMapper.ITypeHandler
{
    //public object Parse(Type destinationType, object value)
    //{
    //    if (value == null || value is DBNull) return null;

    //    var t = value.GetType();
    //    if (t.FullName == "MySqlConnector.MySqlDateTime")
    //    {
    //        // use dynamic to avoid compile-time dependency details
    //        dynamic md = value;
    //        try
    //        {
    //            // MySqlDateTime exposes ToDateTime() and IsValidDateTime
    //            bool valid = md.IsValidDateTime;
    //            if (valid) return DateTime.Parse(md.ToString());
    //            // invalid/zero-date: return null for nullable, or DateTime.MinValue for non-nullable
    //            if (Nullable.GetUnderlyingType(destinationType) != null) return null;
    //            return DateTime.MinValue;
    //        }
    //        catch
    //        {
    //            if (Nullable.GetUnderlyingType(destinationType) != null) return null;
    //            return DateTime.MinValue;
    //        }
    //    }

    //    // fallback conversion
    //    return Convert.ChangeType(value, Nullable.GetUnderlyingType(destinationType) ?? destinationType);
    //}
    public object Parse(Type destinationType, object value)
    {
        if (value == null || value is DBNull) return null;

        var destNullableType = Nullable.GetUnderlyingType(destinationType);
        var destType = destNullableType ?? destinationType;

        var t = value.GetType();
        if (t.FullName == "MySqlConnector.MySqlDateTime")
        {
            try
            {
                // check validity flags (name can vary by version)
                var isValidProp = t.GetProperty("IsValidDateTime", BindingFlags.Public | BindingFlags.Instance)
                                  ?? t.GetProperty("IsValid", BindingFlags.Public | BindingFlags.Instance);
                if (isValidProp != null)
                {
                    var isValid = (bool)isValidProp.GetValue(value);
                    if (!isValid) return destNullableType != null ? null : DateTime.MinValue;
                }

                // Try instance method ToDateTime()
                var toDt = t.GetMethod("ToDateTime", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (toDt != null)
                {
                    var dt = (DateTime)toDt.Invoke(value, null);
                    return dt;
                }

                // Try implicit/explicit conversion operators declared on the type
                var op = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                          .FirstOrDefault(m => (m.Name == "op_Explicit" || m.Name == "op_Implicit")
                                               && m.ReturnType == typeof(DateTime)
                                               && m.GetParameters().Length == 1
                                               && m.GetParameters()[0].ParameterType == t);
                if (op != null)
                {
                    var dt = (DateTime)op.Invoke(null, new[] { value });
                    return dt;
                }

                // Fallback: Convert or parse
                if (value is IConvertible)
                {
                    return Convert.ChangeType(value, destType, CultureInfo.InvariantCulture);
                }

                if (destType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                        return parsed;
                }
            }
            catch
            {
                // swallow and fallback below
            }

            return destNullableType != null ? null : DateTime.MinValue;
        }

        // generic fallback
        return Convert.ChangeType(value, destType, CultureInfo.InvariantCulture);
    }

    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.Value = value ?? DBNull.Value;
    }
}
