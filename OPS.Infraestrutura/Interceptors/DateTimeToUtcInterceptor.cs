using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OPS.Infraestrutura.Interceptors
{
    public class DateTimeToUtcInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ConvertDateTimePropertiesToUtc(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ConvertDateTimePropertiesToUtc(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void ConvertDateTimePropertiesToUtc(DbContext? context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var properties = entry.Properties
                    .Where(p => p.Metadata.ClrType == typeof(DateTime) ||
                               p.Metadata.ClrType == typeof(DateTime?) ||
                               p.Metadata.ClrType == typeof(DateOnly) ||
                               p.Metadata.ClrType == typeof(DateOnly?));

                foreach (var property in properties)
                {
                    if (property.CurrentValue is DateTime dateTime)
                    {
                        DateTime converted;
                        // Convert Unspecified DateTime to UTC
                        if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            converted = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        }
                        // Convert Local DateTime to UTC
                        else if (dateTime.Kind == DateTimeKind.Local)
                        {
                            converted = dateTime.ToUniversalTime();
                        }
                        else
                        {
                            converted = dateTime; // Already UTC
                        }

                        // Set converted value directly on entity property
                        var entityProperty = entry.Entity.GetType().GetProperty(property.Metadata.Name);
                        if (entityProperty != null)
                        {
                            entityProperty.SetValue(entry.Entity, converted);
                        }
                        else
                        {
                            // Fallback to EF property setting
                            property.CurrentValue = converted;
                        }
                    }
                }
            }
        }
    }
}
