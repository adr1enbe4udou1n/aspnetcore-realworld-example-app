using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;
using Conduit.Application.Support;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Queries;

public class ArticlesFeedQuery : PagedQuery, IRequest<MultipleArticlesResponse>
{
}

public class ArticlesFeedHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<ArticlesFeedQuery, MultipleArticlesResponse>
{
    public async Task<MultipleArticlesResponse> Handle(ArticlesFeedQuery request, CancellationToken cancellationToken)
    {
        var articles = await context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .AsSplitQuery()
            .HasAuthorsFollowedBy(currentUser.User!)
            .OrderByDescending(x => x.Id)
            .Select(a => a.Map(currentUser.User))
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}