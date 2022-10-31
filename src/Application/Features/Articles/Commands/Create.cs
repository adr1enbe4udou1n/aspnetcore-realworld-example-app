using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Commands;

public class NewArticleDTO
{
    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Body { get; set; } = default!;

    public List<string>? TagList { get; set; }
}

public record NewArticleRequest(NewArticleDTO Article) : IAuthorizationRequest<SingleArticleResponse>;

public class ArticleCreateValidator : AbstractValidator<NewArticleRequest>
{
    public ArticleCreateValidator(IAppDbContext context, ISlugifier slugifier)
    {
        RuleFor(x => x.Article.Title).NotNull().NotEmpty();
        RuleFor(x => x.Article.Description).NotNull().NotEmpty();
        RuleFor(x => x.Article.Body).NotNull().NotEmpty();

        RuleFor(x => x.Article.Title).MustAsync(
            async (title, cancellationToken) => !await context.Articles
                .Where(x => x.Slug == slugifier.Generate(title!))
                .AnyAsync(cancellationToken)
        )
            .WithMessage("Slug with this title already used");
    }
}

public class ArticleCreateHandler : IAuthorizationRequestHandler<NewArticleRequest, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public ArticleCreateHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SingleArticleResponse> Handle(NewArticleRequest request, CancellationToken cancellationToken)
    {
        var article = _mapper.Map<Article>(request.Article);

        if (request.Article.TagList != null)
        {
            var existingTags = await _context.Tags
                .Where(
                    x => request.Article.TagList.Any(t => t == x.Name)
                )
                .ToListAsync(cancellationToken);

            article.AddTags(existingTags, request.Article.TagList);
        }

        await _context.Articles.AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(_mapper.Map<ArticleDTO>(article));
    }
}