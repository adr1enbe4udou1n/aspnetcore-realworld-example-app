using Conduit.Application.Extensions;
using Conduit.Application.Features.Comments.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

using MediatR;

namespace Conduit.Application.Features.Comments.Commands;

public class NewCommentDto
{

    public required string Body { get; set; }
}

public record SingleCommentResponse(CommentDto Comment);

public record NewCommentCommand(string Slug, NewCommentDto Comment) : IRequest<SingleCommentResponse>;

public class CommentCreateValidator : AbstractValidator<NewCommentCommand>
{
    public CommentCreateValidator()
    {
        RuleFor(x => x.Comment.Body).NotNull().NotEmpty();
    }
}

public class CommentCreateHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<NewCommentCommand, SingleCommentResponse>
{
    public async Task<SingleCommentResponse> Handle(NewCommentCommand request, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comment = new Comment
        {
            Body = request.Comment.Body,
            Article = article,
            Author = currentUser.User!
        };

        await context.Comments.AddAsync(comment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new SingleCommentResponse(comment.Map(currentUser.User));
    }
}