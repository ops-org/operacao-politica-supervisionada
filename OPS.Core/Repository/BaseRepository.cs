using System.Data;

namespace OPS.Core
{
    public abstract class BaseRepository
    {
        public readonly IDbConnection _connection;

        public BaseRepository(IDbConnection connection)
        {
            _connection = connection;
        }
    }
}
