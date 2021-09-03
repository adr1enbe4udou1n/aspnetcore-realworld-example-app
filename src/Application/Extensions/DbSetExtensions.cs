using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Extensions
{
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
    }
}