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
    public ArticleCreateValidator()
    {
        RuleFor(x => x.Title).NotNull().NotEmpty();
        RuleFor(x => x.Description).NotNull().NotEmpty();
        RuleFor(x => x.Body).NotNull().NotEmpty();
    }
}

public class ArticleUpdateValidator : AbstractValidator<UpdateArticleDto>
{
    public ArticleUpdateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().When(x => x.Title != null);
        RuleFor(x => x.Description).NotEmpty().When(x => x.Description != null);
        RuleFor(x => x.Body).NotEmpty().When(x => x.Body != null);
        RuleFor(x => x.TagList).NotNull().When(x => x.TagListSpecified);
    }
}

public class CommandArticles(IAppDbContext context, ICurrentUser currentUser, ISlugifier slugifier, IValidator<NewArticleDto> createValidator, IValidator<UpdateArticleDto> updateValidator) : ICommandArticles
{
    public async Task<SingleArticleResponse> Create(NewArticleDto newArticle, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(newArticle, cancellationToken);

        var baseSlug = slugifier.Generate(newArticle.Title);
        var slug = baseSlug;
        var suffix = 1;

        while (await context.Articles.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        var article = new Article
        {
            Title = newArticle.Title,
            Description = newArticle.Description,
            Body = newArticle.Body,
            Author = currentUser.User!,
            Slug = slug
        };

        if (newArticle.TagList.Count > 0)
        {
            var existingTags = await context.Tags
                .Where(
                    x => newArticle.TagList.Contains(x.Name)
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

        var article = await context.Articles
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .FindAsync(x => x.Slug == slug, "article", cancellationToken);

        if (article.AuthorId != currentUser.User!.Id)
        {
            throw new ForbiddenException("article");
        }

        article.Title = updateArticle.Title ?? article.Title;
        article.Description = updateArticle.Description ?? article.Description;
        article.Body = updateArticle.Body ?? article.Body;

        if (updateArticle.TagListSpecified)
        {
            var existingTags = await context.Tags
                .Where(x => updateArticle.TagList!.Contains(x.Name))
                .ToListAsync(cancellationToken);

            article.SetTags(existingTags, updateArticle.TagList!.ToArray());
        }

        context.Articles.Update(article);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(currentUser.User));
    }

    public async Task Delete(string slug, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == slug, "article", cancellationToken);

        if (article.AuthorId != currentUser.User!.Id)
        {
            throw new ForbiddenException("article");
        }

        context.Articles.Remove(article);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SingleArticleResponse> Favorite(string slug, bool favorite, CancellationToken cancellationToken)
    {
        var article = await context.Articles
            .FindAsync(x => x.Slug == slug, "article", cancellationToken);

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