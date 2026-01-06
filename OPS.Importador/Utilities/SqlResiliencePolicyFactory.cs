using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Registry;

namespace OPS.Importador.Utilities
{
    public class SqlResiliencePolicyFactory
    {
        private readonly ISet<string> _transientDbErrors = new HashSet<string>(new[] { "57014", "57P01", "57P02", "57P03", "58030", "53300" });
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public SqlResiliencePolicyFactory(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IPolicyRegistry<string> GetSqlResiliencePolicies(int transientErrorRetries = 3)
        {
            return new PolicyRegistry
        {
            {
                "DbDeadLockResilience",
                Policy
                    .Handle<NpgsqlException>(ex => _transientDbErrors.Contains(ex.SqlState))
                    .WaitAndRetry(
                        retryCount: transientErrorRetries,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(attempt * 100),
                        onRetry: LogRetryAction)
            },
            //{
            //    "DbDeadLockResilienceAsync",
            //    Policy
            //        .Handle<MySqlException>(ex => _transientDbErrors.Contains(ex.Number))
            //        .WaitAndRetryAsync(
            //            retryCount: transientErrorRetries,
            //            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(attempt * 100),
            //            onRetry: LogRetryAction)
            //}
        };
        }

        private void LogRetryAction(Exception exception, TimeSpan sleepTime, int reattemptCount, Context context) =>
            _logger.Log(
                LogLevel.Warning,
                exception,
                @$"Transient DB Failure while executing query,
                error number: {((NpgsqlException)exception).SqlState};
                reattempt number: {reattemptCount}");
    }
}
