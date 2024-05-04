using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Features.Comments.Commands;

public record CommentDeleteCommand(string Slug, int Id) : IRequest;

public class CommentDeleteHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<CommentDeleteCommand>
{
    public async Task Handle(CommentDeleteCommand request, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);
        var comment = await context.Comments.FindAsync(
            x => x.Id == request.Id && x.ArticleId == article.Id,
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