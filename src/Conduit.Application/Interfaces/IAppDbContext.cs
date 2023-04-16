using Conduit.Domain.Entities;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Interfaces;

public interface IAppDbContext : IDisposable
{
    DbSet<User> Users { get; }
    DbSet<Article> Articles { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tag> Tags { get; }

    void UseRoConnection();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<TResponse> UseTransactionAsync<TResponse>(RequestHandlerDelegate<TResponse> request, CancellationToken cancellationToken = default);
}