using Conduit.Application.Extensions;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using MediatR;

namespace Conduit.Application.Features.Articles.Commands;

public record ArticleFavoriteCommand(string Slug, bool Favorite) : IRequest<SingleArticleResponse>;

public class ArticleFavoriteHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<ArticleFavoriteCommand, SingleArticleResponse>
{
    public async Task<SingleArticleResponse> Handle(ArticleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var article = await context.Articles
            .FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (request.Favorite)
        {
            article.AddFavorite(currentUser.User!);
        }
        else
        {
            article.RemoveFavorite(currentUser.User!);
        }

        context.Articles.Update(article);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }
}