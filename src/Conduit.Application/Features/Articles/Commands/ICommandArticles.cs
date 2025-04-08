using Conduit.Application.Features.Articles.Queries;

namespace Conduit.Application.Features.Articles.Commands;

public interface ICommandArticles
{
    Task<SingleArticleResponse> Create(NewArticleDto newArticle, CancellationToken cancellationToken);
    Task<SingleArticleResponse> Update(string slug, UpdateArticleDto updateArticle, CancellationToken cancellationToken);
    Task Delete(string slug, CancellationToken cancellationToken);
    Task<SingleArticleResponse> Favorite(string slug, bool favorite, CancellationToken cancellationToken);
}
