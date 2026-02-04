using System;
using System.Data.Common;

namespace OPS.Core.Utilities;

public static class DbExtensions
{
    public static int GetTotalRowsFound(this DbDataReader reader)
    {
        reader.NextResult();
        if (reader.Read())
            return reader.GetInt32(0);

        throw new NotImplementedException();
    }
}
