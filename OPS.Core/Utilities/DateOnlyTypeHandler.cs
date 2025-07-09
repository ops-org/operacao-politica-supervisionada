using System;
using System.Data;
using Dapper;

namespace OPS.Core.Utilities
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value) => DateOnly.FromDateTime(Convert.ToDateTime(value.ToString()));

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value;
        }
    }
}
