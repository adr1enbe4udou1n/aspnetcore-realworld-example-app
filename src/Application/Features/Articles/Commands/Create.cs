using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Commands;

public class NewArticleDTO
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Body { get; set; }

    public List<string> TagList { get; set; } = new();
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
                .Where(x => x.Slug == slugifier.Generate(title))
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
        var existingTags = await _context.Tags
            .AsTracking()
            .Where(
                x => request.Article.TagList.Any(t => t == x.Name)
            )
            .ToListAsync(cancellationToken);

        article.AuthorId = _currentUser.User.Id;
        article.Slug = _slugifier.Generate(request.Article.Title);

        article.Tags = request.Article.TagList
            .Where(x => !String.IsNullOrEmpty(x))
            .Select(x =>
            {
                var tag = existingTags.FirstOrDefault(t => t.Name == x);

                return new ArticleTag
                {
                    Tag = tag == null ? new Tag { Name = x } : tag
                };
            })
            .ToList();

        await _context.Articles.AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(_mapper.Map<ArticleDTO>(article));
    }
}
