using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Features.Comments.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

namespace Conduit.Application.Features.Comments.Commands;

public record SingleCommentResponse(CommentDto Comment);


public class CommentCreateValidator : AbstractValidator<NewCommentDto>
{
    public CommentCreateValidator()
    {
        RuleFor(x => x.Body).NotNull().NotEmpty();
    }
}

public record CommentDeleteCommand(string Slug, int Id);

public class CommandComments(IAppDbContext context, ICurrentUser currentUser, IValidator<NewCommentDto> createValidator) : ICommandComments
{
    public async Task<SingleCommentResponse> Create(string slug, NewCommentDto newComment, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(newComment, cancellationToken);

        var article = await context.Articles.FindAsync(x => x.Slug == slug, cancellationToken);

        var comment = new Comment
        {
            Body = newComment.Body,
            Article = article,
            Author = currentUser.User!
        };

        await context.Comments.AddAsync(comment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleCommentResponse(comment.Map(currentUser.User));
    }

    public async Task Delete(string slug, int id, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == slug, cancellationToken);
        var comment = await context.Comments.FindAsync(
            x => x.Id == id && x.ArticleId == article.Id,
            cancellationToken
        );

        if (article.AuthorId != currentUser.User!.Id && comment.AuthorId != currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);
    }
}