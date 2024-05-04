using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;
using Conduit.Application.Support;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Queries;

public record MultipleArticlesResponse(IEnumerable<ArticleDto> Articles, int ArticlesCount);

public class ArticlesListQuery : PagedQuery, IRequest<MultipleArticlesResponse>
{
    public string? Author { get; set; }
    public string? Favorited { get; set; }
    public string? Tag { get; set; }
}

public class ArticlesListHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<ArticlesListQuery, MultipleArticlesResponse>
{
    public async Task<MultipleArticlesResponse> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
    {
        var articles = await context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .AsSplitQuery()
            .FilterByAuthor(request.Author)
            .FilterByTag(request.Tag)
            .FilterByFavoritedBy(request.Favorited)
            .OrderByDescending(x => x.Id)
            .Select(a => a.Map(currentUser.User))
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}