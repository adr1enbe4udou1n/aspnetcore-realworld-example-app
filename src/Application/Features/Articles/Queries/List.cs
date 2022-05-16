using Application.Diagnostics;
using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using Application.Support;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Application.Features.Articles.Queries;

public class ArticleDTO
{
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

public class ArticlesListQuery : PagedQuery, IRequest<MultipleArticlesResponse>
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

public class ArticlesListHandler : IRequestHandler<ArticlesListQuery, MultipleArticlesResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public ArticlesListHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<MultipleArticlesResponse> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
    {
        using var mediatorActivity = Telemetry.ApplicationActivitySource.StartActivity("ArticlesListHandler.Handle");

        var articles = await _context.Articles
            .FilterByAuthor(request.Author)
            .FilterByTag(request.Tag)
            .FilterByFavoritedBy(request.Favorited)
            .OrderByDescending(x => x.Id)
            .ProjectTo<ArticleDTO>(_mapper.ConfigurationProvider, new
            {
                currentUser = _currentUser.User
            })
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}