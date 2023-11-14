using Conduit.Application.Extensions;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using MediatR;

namespace Conduit.Application.Features.Articles.Commands;

public record ArticleFavoriteCommand(string Slug, bool Favorite) : IRequest<SingleArticleResponse>;

public class ArticleFavoriteHandler : IRequestHandler<ArticleFavoriteCommand, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticleFavoriteHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SingleArticleResponse> Handle(ArticleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles
            .FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (request.Favorite)
        {
            _context.ArticleFavorite.Add(new ArticleFavorite
            {
                Article = article,
                User = _currentUser.User!
            });
        }
        else
        {
            _context.ArticleFavorite.Remove(
                article.FavoredUsers.First(x => x.UserId == _currentUser.User!.Id)
            );
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(_currentUser.User));
    }
}