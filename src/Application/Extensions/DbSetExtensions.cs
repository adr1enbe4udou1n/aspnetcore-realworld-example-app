using System.Linq.Expressions;
using Application.Exceptions;
using Application.Support;
using Microsoft.EntityFrameworkCore;

namespace Application.Extensions;

public static class DbSetExtensions
{
    public static async Task<TSource> FindAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entity = await source.Where(predicate).SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException();
        }

        return entity;
    }

    public static async Task<PagedResponse<TSource>> PaginateAsync<TSource>(
        this IQueryable<TSource> source,
        PagedQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var count = source.CountAsync(cancellationToken);
        var items = source
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(count, items);

        return new PagedResponse<TSource>
        {
            Items = items.Result,
            Total = count.Result
        };
    }
}