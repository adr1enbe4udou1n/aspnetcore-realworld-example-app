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
    private readonly ICurrentUser _currentUser;
    private readonly ISlugifier _slugifier;

    public ArticleCreateHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser, ISlugifier slugifier)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _slugifier = slugifier;
    }

    public async Task<SingleArticleResponse> Handle(NewArticleRequest request, CancellationToken cancellationToken)
    {
        var article = _mapper.Map<Article>(request.Article);

        article.Author = _currentUser.User!;
        article.Slug = _slugifier.Generate(request.Article.Title);

        if (request.Article.TagList != null)
        {
            var existingTags = await _context.Tags
                .Where(
                    x => request.Article.TagList.Any(t => t == x.Name)
                )
                .ToListAsync(cancellationToken);

            article.Tags = request.Article.TagList
                .Where(x => !String.IsNullOrEmpty(x))
                .Distinct()
                .Select(x =>
                {
                    var tag = existingTags.FirstOrDefault(t => t.Name == x);

                    return new ArticleTag
                    {
                        Tag = tag == null ? new Tag { Name = x } : tag
                    };
                })
                .ToList();
        }

        await _context.Articles.AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(_mapper.Map<ArticleDTO>(article));
    }
}