using System;
using System.Data;
using Dapper;

namespace OPS.Core.Utilities
{
    public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override TimeOnly Parse(object value) => TimeOnly.FromDateTime((DateTime)value);

        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value;
        }
    }
}
