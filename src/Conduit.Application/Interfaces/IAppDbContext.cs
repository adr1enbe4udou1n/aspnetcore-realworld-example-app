using Conduit.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Interfaces;

public interface IAppDbContext : IDisposable
{
    DbSet<User> Users { get; }
    DbSet<Article> Articles { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tag> Tags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}