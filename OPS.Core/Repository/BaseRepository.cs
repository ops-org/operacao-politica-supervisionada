using MySqlConnector;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Core
{
    public abstract class BaseRepository
    {
        public readonly ILogger _logger;
        public readonly IDbConnection _connection;

        public BaseRepository(ILogger logger, IDbConnection connection)
        {
            _connection = connection;
            _logger = logger;
        }
    }
}
