using Conduit.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Conduit.Infrastructure.Interceptors;

public class AuditableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var entries = eventData.Context!.ChangeTracker.Entries<IAuditableEntity>();
        foreach (var entry in entries)
        {
            var utcNow = TruncateToMicroseconds(DateTime.UtcNow);

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(a => a.CreatedAt).CurrentValue = utcNow;
                    entry.Property(a => a.UpdatedAt).CurrentValue = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(a => a.UpdatedAt).CurrentValue = utcNow;
                    break;
            }
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }

    private static DateTime TruncateToMicroseconds(DateTime value)
    {
        return new DateTime(value.Ticks - value.Ticks % 10, value.Kind);
    }
}