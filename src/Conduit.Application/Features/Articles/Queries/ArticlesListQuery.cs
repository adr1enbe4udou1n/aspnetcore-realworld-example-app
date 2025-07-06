using Conduit.Application.Support;

namespace Conduit.Application.Features.Articles.Queries;

public class ArticlesListQuery : PagedQuery
{
    public string? Author { get; set; }
    public string? Favorited { get; set; }
    public string? Tag { get; set; }
}