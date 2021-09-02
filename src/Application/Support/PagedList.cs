using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Support
{
    public class PagedQuery
    {
        public const int MaxLimit = 20;

        public int? Limit { get; set; }

        public int? Offset { get; set; }

        // public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        // {
        //     var count = await source.CountAsync();
        //     var items = await source
        //         .Skip((pageNumber - 1) * pageSize)
        //         .Take(Limit)
        //         .ToListAsync();

        //     return new PagedList<T>(items, count, pageNumber, pageSize);
        // }
    }
}