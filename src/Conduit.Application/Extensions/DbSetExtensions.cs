using System.Linq.Expressions;

using Conduit.Application.Exceptions;
using Conduit.Application.Support;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Extensions;

public static class DbSetExtensions
{
    public static async Task<TSource> FindAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await source.Where(predicate).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException();
    }

    public static async Task<PagedResponse<TResult>> PaginateAsync<TSource, TResult>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TResult>> projection,
        PagedQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip(query.Offset ?? 0)
            .Take(query.Limit > PagedQuery.MaxLimit
                ? PagedQuery.MaxLimit
                : query.Limit ?? PagedQuery.MaxLimit)
            .Select(projection)
            .ToListAsync(cancellationToken);

        return new PagedResponse<TResult>(items, count);
    }
}