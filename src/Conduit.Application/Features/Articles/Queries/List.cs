using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;
using Conduit.Application.Support;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Queries;

public record MultipleArticlesResponse(IEnumerable<ArticleDto> Articles, int ArticlesCount);

public class ArticlesListQuery : PagedQuery, IRequest<MultipleArticlesResponse>
{
    /// <summary>
    /// Filter by author (username)
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Filter by favorites of a user (username)
    /// </summary>
    public string? Favorited { get; set; }

    /// <summary>
    /// Filter by tag
    /// </summary>
    public string? Tag { get; set; }
}

public class ArticlesListHandler : IRequestHandler<ArticlesListQuery, MultipleArticlesResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticlesListHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MultipleArticlesResponse> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
    {
        var articles = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .AsSplitQuery()
            .FilterByAuthor(request.Author)
            .FilterByTag(request.Tag)
            .FilterByFavoritedBy(request.Favorited)
            .OrderByDescending(x => x.Id)
            .Select(a => a.Map(_currentUser.User))
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}