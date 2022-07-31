using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence;

public class AppDbContext : BaseDbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        ChangeTracker.StateChanged += UpdateTimestamps;
        ChangeTracker.Tracked += UpdateTimestamps;
    }

    private void UpdateTimestamps(object? sender, EntityEntryEventArgs e)
    {
        if (e.Entry.Entity is IHasTimestamps entity)
        {
            switch (e.Entry.State)
            {
                case EntityState.Added:
                    if (entity.CreatedAt == default)
                    {
                        entity.CreatedAt = DateTime.UtcNow;
                    }
                    entity.UpdatedAt = entity.CreatedAt;
                    break;
                case EntityState.Modified:
                    if (entity.UpdatedAt == default)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
            }
        }
    }


}