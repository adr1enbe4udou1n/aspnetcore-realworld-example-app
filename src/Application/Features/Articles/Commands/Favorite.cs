using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;

namespace Application.Features.Articles.Commands;

public record ArticleFavoriteRequest(string Slug, bool Favorite) : ICommand<SingleArticleResponse>;

public class ArticleFavoriteHandler : ICommandHandler<ArticleFavoriteRequest, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticleFavoriteHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SingleArticleResponse> Handle(ArticleFavoriteRequest request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles
            .FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (request.Favorite)
        {
            article.Favorite(_currentUser.User!);
        }
        else
        {
            article.Unfavorite(_currentUser.User!);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(_currentUser.User));
    }
}