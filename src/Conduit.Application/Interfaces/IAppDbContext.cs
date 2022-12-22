using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IAppDbContext : IDisposable
{
    DbSet<User> Users { get; }
    DbSet<Article> Articles { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tag> Tags { get; }

    void UseRoConnection();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}