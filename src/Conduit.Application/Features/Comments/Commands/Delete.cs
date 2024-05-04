using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Features.Comments.Commands;

public record CommentDeleteCommand(string Slug, int Id) : IRequest;

public class CommentDeleteHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<CommentDeleteCommand>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task Handle(CommentDeleteCommand request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);
        var comment = await _context.Comments.FindAsync(
            x => x.Id == request.Id && x.ArticleId == article.Id,
            cancellationToken
        );

        if (article.AuthorId != _currentUser.User!.Id && comment.AuthorId != _currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}