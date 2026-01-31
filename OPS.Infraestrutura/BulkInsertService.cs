using System;
using System.Collections.Generic;
using System.Text;

namespace OPS.Infraestrutura;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Service for bulk inserting data with EF Core while disabling change tracking for performance
/// </summary>
public class BulkInsertService<TEntity> where TEntity : class
{
    /// <summary>
    /// Bulk insert without clearing tracking (faster but uses more memory)
    /// </summary>
    public void BulkInsertNoTracking(DbContext context, IEnumerable<TEntity> entities, int batchSize = 500)
    {
        var entityList = entities.ToList();

        if (!entityList.Any())
            return;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var originalAutoDetectChanges = context.ChangeTracker.AutoDetectChangesEnabled;
            var originalLazyLoadingEnabled = context.ChangeTracker.LazyLoadingEnabled;
            var originalQueryTrackingBehavior = context.ChangeTracker.QueryTrackingBehavior;

            // Disable tracking for maximum performance
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            context.ChangeTracker.LazyLoadingEnabled = false;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var batches = entityList
                .Select((entity, index) => new { entity, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.entity).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                context.AddRange(batch);
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }

            stopwatch.Stop();
            Console.WriteLine($"Inserted {entityList.Count} records in {stopwatch.ElapsedMilliseconds}ms");

            // Restore original settings
            context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetectChanges;
            context.ChangeTracker.LazyLoadingEnabled = originalLazyLoadingEnabled;
            context.ChangeTracker.QueryTrackingBehavior = originalQueryTrackingBehavior;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during bulk insert: {ex.Message}");
            throw;
        }
    }
}
