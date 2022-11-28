using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using Application.Support;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Queries;

public class ArticleDTO
{
    public ArticleDTO()
    {
    }

    public ArticleDTO(Article article, User? currentUser)
    {
        Slug = article.Slug;
        Title = article.Title;
        Description = article.Description;
        Body = article.Body;
        TagList = article.Tags.Select(t => t.Tag.Name).OrderBy(t => t);
        CreatedAt = article.CreatedAt;
        UpdatedAt = article.UpdatedAt;
        Favorited = currentUser != null && currentUser.HasFavorite(article);
        FavoritesCount = article.FavoredUsers.Count;
        Author = new ProfileDTO(article.Author, currentUser);
    }

    public string Title { get; set; } = default!;

    public string Slug { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Body { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IEnumerable<string> TagList { get; set; } = default!;

    public ProfileDTO Author { get; set; } = null!;

    public bool Favorited { get; set; }

    public int FavoritesCount { get; set; }
}

public record MultipleArticlesResponse(IEnumerable<ArticleDTO> Articles, int ArticlesCount);

public class ArticlesListQuery : PagedQuery, IQuery<MultipleArticlesResponse>
{
    /// <summary>
    /// Filter by author (username)
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Filter by favorites of a user (username)
    /// </summary>
    public string? Favorited { get; set; }

    /// <summary>
    /// Filter by tag
    /// </summary>
    public string? Tag { get; set; }
}

public class ArticlesListHandler : IQueryHandler<ArticlesListQuery, MultipleArticlesResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticlesListHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MultipleArticlesResponse> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
    {
        await _currentUser.LoadFollowing();
        await _currentUser.LoadFavoriteArticles();

        var articles = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.FavoredUsers)
            .FilterByAuthor(request.Author)
            .FilterByTag(request.Tag)
            .FilterByFavoritedBy(request.Favorited)
            .OrderByDescending(x => x.Id)
            .Select(a => new ArticleDTO(a, _currentUser.User))
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}