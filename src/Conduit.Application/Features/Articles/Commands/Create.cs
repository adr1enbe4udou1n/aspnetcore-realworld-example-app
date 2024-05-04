using System.Collections.ObjectModel;

using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Commands;

public class NewArticleDto
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string Body { get; set; }

#pragma warning disable CA2227
    public Collection<string> TagList { get; set; } = [];
#pragma warning restore CA2227
}

public record NewArticleCommand(NewArticleDto Article) : IRequest<SingleArticleResponse>;

public class ArticleCreateValidator : AbstractValidator<NewArticleCommand>
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

public class ArticleCreateHandler(IAppDbContext context, ICurrentUser currentUser, ISlugifier slugifier) : IRequestHandler<NewArticleCommand, SingleArticleResponse>
{
    public async Task<SingleArticleResponse> Handle(NewArticleCommand request, CancellationToken cancellationToken)
    {
        var article = new Article
        {
            Title = request.Article.Title,
            Description = request.Article.Description,
            Body = request.Article.Body,
            Author = currentUser.User!,
            Slug = slugifier.Generate(request.Article.Title)
        };

        if (request.Article.TagList.Count > 0)
        {
            var existingTags = await context.Tags
                .Where(
                    x => request.Article.TagList
                        .AsEnumerable()
                        .Any(t => t == x.Name)
                )
                .ToListAsync(cancellationToken);

            article.AddTags(existingTags, request.Article.TagList.ToArray());
        }

        await context.Articles.AddAsync(article, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }
}