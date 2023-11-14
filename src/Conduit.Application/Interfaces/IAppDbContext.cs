using Conduit.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Interfaces;

public interface IAppDbContext : IDisposable
{
    DbSet<User> Users { get; }
    DbSet<FollowerUser> FollowerUser { get; }
    DbSet<Article> Articles { get; }
    DbSet<ArticleFavorite> ArticleFavorite { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tag> Tags { get; }

    void UseRoConnection();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}