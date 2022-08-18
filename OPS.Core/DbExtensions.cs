// https://github.com/dotnet/orleans/blob/master/src/OrleansSQLUtils/Storage/DbExtensions.cs

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
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
        /// <param name="fieldName">The name of the field to retrieve.</param>
        /// <param name="default">The default value if value in position is <see cref="System.DBNull"/>.</param>
        /// <returns>Either the given value or the default for the requested type.</returns>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <remarks>This function throws if the given <see paramref="fieldName"/> does not exist.</remarks>
        public static TValue GetValueOrDefault<TValue>(this IDataRecord record, string fieldName, TValue @default = default(TValue))
        {
            var ordinal = record.GetOrdinal(fieldName);
            return record.IsDBNull(ordinal) ? @default : (TValue)record.GetValue(ordinal);
        }


        /// <summary>
        /// Returns a value if it is not <see cref="System.DBNull"/>, <em>default(TValue)</em> otherwise.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to request.</typeparam>
        /// <param name="record">The record from which to retrieve the value.</param>
        /// <param name="fieldName">The name of the field to retrieve.</param>
        /// <param name="default">The default value if value in position is <see cref="System.DBNull"/>.</param>
        /// <returns>Either the given value or the default for the requested type.</returns>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <remarks>This function throws if the given <see paramref="fieldName"/> does not exist.</remarks>
        public static async Task<TValue> GetValueOrDefaultAsync<TValue>(this DbDataReader record, string fieldName, TValue @default = default(TValue))
        {
            var ordinal = record.GetOrdinal(fieldName);
            return (await record.IsDBNullAsync(ordinal).ConfigureAwait(false)) ? @default : (await record.GetFieldValueAsync<TValue>(ordinal).ConfigureAwait(false));
        }


        /// <summary>
        /// Returns a value if it is not <see cref="System.DBNull"/>, <em>default(TValue)</em> otherwise.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to request.</typeparam>
        /// <param name="record">The record from which to retrieve the value.</param>
        /// <param name="ordinal">The ordinal of the fieldname.</param>
        /// <param name="default">The default value if value in position is <see cref="System.DBNull"/>.</param>
        /// <returns>Either the given value or the default for the requested type.</returns>
        /// <exception cref="IndexOutOfRangeException"/>                
        public static TValue GetValueOrDefault<TValue>(this IDataRecord record, int ordinal, TValue @default = default(TValue))
        {
            return record.IsDBNull(ordinal) ? @default : (TValue)record.GetValue(ordinal);
        }


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


        /// <summary>
        /// Returns a value with the given <see paramref="fieldName"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of value to retrieve.</typeparam>
        /// <param name="record">The record from which to retrieve the value.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>Value in the given field indicated by <see paramref="fieldName"/>.</returns>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <remarks>This function throws if the given <see paramref="fieldName"/> does not exist.</remarks>        
        public static TValue GetValue<TValue>(this IDataRecord record, string fieldName)
        {
            var ordinal = record.GetOrdinal(fieldName);
            return (TValue)record.GetValue(ordinal);
        }


        /// <summary>
        /// Returns a value with the given <see paramref="fieldName"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of value to retrieve.</typeparam>
        /// <param name="record">The record from which to retrieve the value.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="cancellationToken">The cancellation token. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>Value in the given field indicated by <see paramref="fieldName"/>.</returns>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <remarks>This function throws if the given <see paramref="fieldName"/> does not exist.</remarks>        
        public static async Task<TValue> GetValueAsync<TValue>(this DbDataReader record, string fieldName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ordinal = record.GetOrdinal(fieldName);
            return await record.GetFieldValueAsync<TValue>(ordinal, cancellationToken).ConfigureAwait(false);
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
