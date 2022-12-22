using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using Application.Support;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Queries;

public class ArticlesFeedQuery : PagedQuery, IQuery<MultipleArticlesResponse>
{
}

public class ArticlesFeedHandler : IQueryHandler<ArticlesFeedQuery, MultipleArticlesResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticlesFeedHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MultipleArticlesResponse> Handle(ArticlesFeedQuery request, CancellationToken cancellationToken)
    {
        await _currentUser.LoadFollowing();
        await _currentUser.LoadFavoriteArticles();

        var articles = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .HasAuthorsFollowedBy(_currentUser.User!)
            .OrderByDescending(x => x.Id)
            .Select(a => a.Map(_currentUser.User))
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}