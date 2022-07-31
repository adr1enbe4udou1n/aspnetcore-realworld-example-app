using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Application.Interfaces;

public interface IAppBaseDbContext : IDisposable
{
    DbSet<User> Users { get; }
    DbSet<Article> Articles { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tag> Tags { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}