using System.Data;
using Dapper;
using Polly;
using Polly.Registry;

namespace OPS.Importador.Utilities
{
    public static class DapperExtensions
    {
        private static Policy _policy = Policy.NoOp();
        private static IAsyncPolicy _asyncPolicy = Policy.NoOpAsync();

        public static void SetPolicies(IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry)
        {
            _policy = readOnlyPolicyRegistry.Get<Policy>("DbDeadLockResilience");
            //_asyncPolicy = readOnlyPolicyRegistry.Get<IAsyncPolicy>("DbDeadLockResilienceAsync");
        }

        //public static T GetFirstWithRetry<T>(this IDbConnection connection,
        //                                    string? sql = null, object? parameters = null, IDbTransaction? transaction = null) where T : class =>
        //    _policy.Execute(() => connection.GetFirst<T>(sql, parameters, transaction));

        //public static T QueryFirstOrDefaultWithRetry<T>(this IDbConnection connection, string sql,
        //                                      object? parameters = null, IDbTransaction? transaction = null) =>
        //    _policy.Execute(() => connection.QueryFirstOrDefault<T>(sql, parameters, transaction));

        //public static async Task<bool> UpdateAsyncWithRetry<T>(this IDbConnection connection, T entityToUpdate, IEnumerable<string> columnsToUpdate,
        //                                                 IDbTransaction? transaction = null) where T : class =>
        //   await _asyncPolicy.ExecuteAsync(async () => await connection.UpdateAsync(entityToUpdate, columnsToUpdate, transaction));

        public static int ExecuteWithRetry(this IDbConnection cnn, string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _policy.Execute(() => cnn.Execute(sql, param, transaction, commandTimeout, commandType));
        }



        //Similarly, add overloads to all the other methods in existing repo.
    }
}
