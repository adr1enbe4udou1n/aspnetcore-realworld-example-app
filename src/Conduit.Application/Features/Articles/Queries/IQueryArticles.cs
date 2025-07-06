namespace Conduit.Application.Features.Articles.Queries;

public interface IQueryArticles
{
    Task<MultipleArticlesResponse> List(ArticlesListQuery request, CancellationToken cancellationToken);
    Task<MultipleArticlesResponse> Feed(ArticlesFeedQuery request, CancellationToken cancellationToken);
    Task<SingleArticleResponse> Find(string slug, CancellationToken cancellationToken);
}