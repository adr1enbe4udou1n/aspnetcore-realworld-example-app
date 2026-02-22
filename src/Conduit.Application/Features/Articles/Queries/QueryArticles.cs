using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Queries;

public record MultipleArticlesResponse(IEnumerable<ArticleDto> Articles, int ArticlesCount);
public record SingleArticleResponse(ArticleDto Article);


public class QueryArticles(IAppDbContext context, ICurrentUser currentUser) : IQueryArticles
{
    public async Task<MultipleArticlesResponse> List(ArticlesListQuery request, CancellationToken cancellationToken)
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
            .PaginateAsync(a => a.Map(currentUser.User), request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }

    public async Task<MultipleArticlesResponse> Feed(ArticlesFeedQuery request, CancellationToken cancellationToken)
    {
        var articles = await context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .AsSplitQuery()
            .HasAuthorsFollowedBy(currentUser.User!)
            .OrderByDescending(x => x.Id)
            .PaginateAsync(a => a.Map(currentUser.User), request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }


    public async Task<SingleArticleResponse> Find(string slug, CancellationToken cancellationToken)
    {
        var article = await context.Articles
            .FindAsync(x => x.Slug == slug, cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }
}