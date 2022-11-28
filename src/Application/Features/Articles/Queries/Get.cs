using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Mediator;

namespace Application.Features.Articles.Queries;

public record SingleArticleResponse(ArticleDTO Article);

public record ArticleGetQuery(string Slug) : IQuery<SingleArticleResponse>;

public class ArticleGetHandler : IQueryHandler<ArticleGetQuery, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticleGetHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SingleArticleResponse> Handle(ArticleGetQuery request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles
            .FindAsync(x => x.Slug == request.Slug, cancellationToken);

        return new SingleArticleResponse(new ArticleDTO(article, _currentUser.User));
    }
}