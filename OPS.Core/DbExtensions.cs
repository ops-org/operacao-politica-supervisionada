// https://github.com/dotnet/orleans/blob/master/src/OrleansSQLUtils/Storage/DbExtensions.cs

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace OPS.Core
{
    /// <summary>
    /// Contains some convenience methods to use in conjunction with <see cref="IRelationalStorage">IRelationalStorage</see> and <see cref="RelationalStorage">GenericRelationalStorage</see>.
    /// </summary>
    public static class DbExtensions
    {
        /// <summary>
        /// Returns a value if it is not <see cref="System.DBNull"/>, <em>default(TValue)</em> otherwise.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to request.</typeparam>
        /// <param name="record">The record from which to retrieve the value.</param>
        /// <param name="ordinal">The ordinal of the fieldname.</param>
        /// <param name="default">The default value if value in position is <see cref="System.DBNull"/>.</param>
        /// <returns>Either the given value or the default for the requested type.</returns>
        /// <exception cref="IndexOutOfRangeException"/>                
        public static async Task<TValue> GetValueOrDefaultAsync<TValue>(this DbDataReader record, int ordinal, TValue @default = default(TValue))
        {
            return (await record.IsDBNullAsync(ordinal).ConfigureAwait(false)) ? @default : (await record.GetFieldValueAsync<TValue>(ordinal).ConfigureAwait(false));
        }


        public static int GetTotalRowsFound(this DbDataReader reader)
        {
            reader.NextResult();
            if (reader.Read())
                return reader.GetInt32(0);

            throw new Exception();
        }
    }
}
