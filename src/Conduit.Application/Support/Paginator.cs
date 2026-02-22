using System.Collections.ObjectModel;

namespace Conduit.Application.Support;

public class PagedQuery
{
    public const int MaxLimit = 20;

    public int? Limit { get; set; }

    public int? Offset { get; set; }
}

public class PagedResponse<T>(IEnumerable<T> items, int total)
{
    public Collection<T> Items { get; } = new Collection<T>(items.ToList());

    public int Total { get; } = total;
}