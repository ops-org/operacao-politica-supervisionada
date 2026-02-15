using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OPS.Core.DTOs;
using OPS.Infraestrutura;

namespace OPS.Core.Repositories
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

        protected async Task<GraficoBarraDTO> GastosPorAno(int id, string strSql)
        {
            var categories = new List<int>();
            var series = new List<decimal>();
            var series2 = new List<decimal>();

            var indices = await _context.IndicesInflacao
                .OrderBy(i => i.Ano).ThenBy(i => i.Mes)
                .ToListAsync();

            var lastIndice = indices.LastOrDefault()?.Indice ?? 1;

            var gastosMensais = new List<(int Ano, int Mes, decimal Valor)>();

            using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
            {
                while (await reader.ReadAsync())
                {
                    gastosMensais.Add((
                        Convert.ToInt32(reader["ano"]),
                        Convert.ToInt32(reader["mes"]),
                        Convert.ToDecimal(reader["valor_total"])
                    ));
                }
            }

            var anos = gastosMensais.Select(g => g.Ano).Distinct().OrderBy(a => a).ToList();

            foreach (var ano in anos)
            {
                categories.Add(ano);

                var gastosDoAno = gastosMensais.Where(g => g.Ano == ano).ToList();
                decimal totalOriginal = gastosDoAno.Sum(g => g.Valor);
                series.Add(totalOriginal);

                decimal totalDeflacionado = 0;
                foreach (var gasto in gastosDoAno)
                {
                    var indiceMes = indices.FirstOrDefault(i => i.Ano == gasto.Ano && i.Mes == gasto.Mes)?.Indice ?? 0;
                    if (indiceMes > 0)
                    {
                        totalDeflacionado += gasto.Valor * (lastIndice / indiceMes);
                    }
                    else
                    {
                        totalDeflacionado += gasto.Valor;
                    }
                }
                series2.Add(totalDeflacionado);
            }

            return new GraficoBarraDTO
            {
                Categories = categories,
                Series = series,
                Series2 = series2
            };
        }
    }
}
