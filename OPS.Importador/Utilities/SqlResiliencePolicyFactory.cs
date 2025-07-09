using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Polly;
using Polly.Registry;

namespace OPS.Importador.Utilities
{
    public class SqlResiliencePolicyFactory
    {
        private readonly ISet<int> _transientDbErrors = new HashSet<int>(new[] { 1205 });
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
                    .Handle<MySqlException>(ex => _transientDbErrors.Contains(ex.Number))
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
                error number: {((MySqlException)exception).Number};
                reattempt number: {reattemptCount}");
    }
}
