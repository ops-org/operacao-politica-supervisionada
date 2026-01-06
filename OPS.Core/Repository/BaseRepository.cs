using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OPS.Infraestrutura;

namespace OPS.Core.Repository
{
    public class BaseRepository
    {
        protected readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        // Helper methods to execute raw SQL when needed
        protected async Task<DbDataReader> ExecuteReaderAsync(string sql, object parameters = null)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            
            var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(ConvertAnonymousToParameters(parameters));
            }

            return await command.ExecuteReaderAsync();
        }

        public static NpgsqlParameter[] ConvertAnonymousToParameters(object anonymousObject)
        {
            var properties = anonymousObject.GetType().GetProperties();
            var parameters = new List<NpgsqlParameter>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(anonymousObject) ?? DBNull.Value;
                parameters.Add(new NpgsqlParameter($"@{prop.Name}", value));
            }

            return parameters.ToArray();
        }
    }
}
