using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;
using Conduit.Application.Support;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Queries;

public class ArticlesFeedQuery : PagedQuery, IRequest<MultipleArticlesResponse>
{
}

public class ArticlesFeedHandler : IRequestHandler<ArticlesFeedQuery, MultipleArticlesResponse>
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
        var articles = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .AsSplitQuery()
            .HasAuthorsFollowedBy(_currentUser.User!)
            .OrderByDescending(x => x.Id)
            .Select(a => a.Map(_currentUser.User))
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}