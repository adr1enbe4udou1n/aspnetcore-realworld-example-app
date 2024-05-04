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
            context.ArticleFavorite.Add(new ArticleFavorite
            {
                Article = article,
                User = currentUser.User!
            });
        }
        else
        {
            context.ArticleFavorite.Remove(
                article.FavoredUsers.First(x => x.UserId == currentUser.User!.Id)
            );
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }
}