using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Articles.Commands;

public class ArticleCreateValidator : AbstractValidator<NewArticleDto>
{
    public ArticleCreateValidator(IAppDbContext context, ISlugifier slugifier)
    {
        RuleFor(x => x.Title).NotNull().NotEmpty();
        RuleFor(x => x.Description).NotNull().NotEmpty();
        RuleFor(x => x.Body).NotNull().NotEmpty();

        RuleFor(x => x.Title).MustAsync(
            async (title, cancellationToken) => !await context.Articles
                .Where(x => x.Slug == slugifier.Generate(title!))
                .AnyAsync(cancellationToken)
        )
            .WithMessage("Slug with this title already used");
    }
}

public class ArticleUpdateValidator : AbstractValidator<UpdateArticleDto>
{
    public ArticleUpdateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().When(x => x.Title != null);
        RuleFor(x => x.Description).NotEmpty().When(x => x.Description != null);
        RuleFor(x => x.Body).NotEmpty().When(x => x.Body != null);
    }
}

public class CommandArticles(IAppDbContext context, ICurrentUser currentUser, ISlugifier slugifier, IValidator<NewArticleDto> createValidator, IValidator<UpdateArticleDto> updateValidator) : ICommandArticles
{
    public async Task<SingleArticleResponse> Create(NewArticleDto newArticle, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(newArticle, cancellationToken);

        var article = new Article
        {
            Title = newArticle.Title,
            Description = newArticle.Description,
            Body = newArticle.Body,
            Author = currentUser.User!,
            Slug = slugifier.Generate(newArticle.Title)
        };

        if (newArticle.TagList.Count > 0)
        {
            var existingTags = await context.Tags
                .Where(
                    x => newArticle.TagList
                        .AsEnumerable()
                        .Any(t => t == x.Name)
                )
                .ToListAsync(cancellationToken);

            article.AddTags(existingTags, newArticle.TagList.ToArray());
        }

        await context.Articles.AddAsync(article, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }

    public async Task<SingleArticleResponse> Update(string slug, UpdateArticleDto updateArticle, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(updateArticle, cancellationToken);

        var article = await context.Articles.FindAsync(x => x.Slug == slug, cancellationToken);

        if (article.AuthorId != currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        article.Title = updateArticle.Title ?? article.Title;
        article.Description = updateArticle.Description ?? article.Description;
        article.Body = updateArticle.Body ?? article.Body;

        context.Articles.Update(article);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }

    public async Task Delete(string slug, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == slug, cancellationToken);

        if (article.AuthorId != currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        context.Articles.Remove(article);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SingleArticleResponse> Favorite(string slug, bool favorite, CancellationToken cancellationToken)
    {
        var article = await context.Articles
            .FindAsync(x => x.Slug == slug, cancellationToken);

        if (favorite)
        {
            article.AddFavorite(currentUser.User!);
        }
        else
        {
            article.RemoveFavorite(currentUser.User!);
        }

        context.Articles.Update(article);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }
}