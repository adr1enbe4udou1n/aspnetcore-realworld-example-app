using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Application.Interfaces.Mediator;
using Conduit.Domain.Entities;

namespace Conduit.Application.Features.Articles.Queries;

public class ArticleDto
{
    public string Title { get; set; } = default!;

    public string Slug { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Body { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IEnumerable<string> TagList { get; set; } = default!;

    public ProfileDto Author { get; set; } = null!;

    public bool Favorited { get; set; }

    public int FavoritesCount { get; set; }
}

public static class ArticleMapper
{
    public static ArticleDto Map(this Article article, User? currentUser)
    {
        return new()
        {
            Slug = article.Slug,
            Title = article.Title,
            Description = article.Description,
            Body = article.Body,
            TagList = article.Tags.Select(t => t.Tag.Name).OrderBy(t => t),
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            Favorited = currentUser != null && currentUser.HasFavorite(article),
            FavoritesCount = article.FavoredUsers.Count,
            Author = article.Author.MapToProfile(currentUser),
        };
    }
}

public record SingleArticleResponse(ArticleDto Article);

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

        return new SingleArticleResponse(article.Map(_currentUser.User));
    }
}