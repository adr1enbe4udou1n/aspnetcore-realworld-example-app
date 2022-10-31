using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Queries;

public record SingleArticleResponse(ArticleDTO Article);

public record ArticleGetQuery(string Slug) : IQuery<SingleArticleResponse>;

public class ArticleGetHandler : IQueryHandler<ArticleGetQuery, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public ArticleGetHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SingleArticleResponse> Handle(ArticleGetQuery request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles
            .Include(x => x.Author)
            .Include(x => x.FavoredUsers)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .FindAsync(x => x.Slug == request.Slug, cancellationToken);

        return new SingleArticleResponse(_mapper.Map<ArticleDTO>(article));
    }
}