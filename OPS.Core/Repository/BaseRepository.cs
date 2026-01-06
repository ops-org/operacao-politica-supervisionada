using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura;

namespace OPS.Core.Repository
{
    public abstract class BaseRepository : IDisposable
    {
        protected readonly AppDbContext _context;
        private bool _disposed = false;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        // Helper methods to execute raw SQL when needed
        protected async Task<DbDataReader> ExecuteReaderAsync(string sql, params object[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@p{i}";
                    parameter.Value = parameters[i] ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            return await command.ExecuteReaderAsync();
        }

        protected async Task<object> ExecuteScalarAsync(string sql, params object[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@p{i}";
                    parameter.Value = parameters[i] ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            return await command.ExecuteScalarAsync();
        }

        protected async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@p{i}";
                    parameter.Value = parameters[i] ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            return await command.ExecuteNonQueryAsync();
        }

        protected async Task<List<Dictionary<string, object>>> ExecuteDictAsync(string sql, params object[] parameters)
        {
            var result = new List<Dictionary<string, object>>();
            
            using (var reader = await ExecuteReaderAsync(sql, parameters))
            {
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    result.Add(row);
                }
            }

            return result;
        }

        public void BeginTransaction()
        {
            if (_context.Database.CurrentTransaction == null)
            {
                _context.Database.BeginTransaction();
            }
        }

        public void CommitTransaction()
        {
            try
            {
                _context.Database.CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
                _disposed = true;
            }
        }
    }
}
