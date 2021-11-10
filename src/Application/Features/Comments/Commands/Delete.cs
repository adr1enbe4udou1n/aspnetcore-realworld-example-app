using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Extensions;
using Application.Features.Comments.Queries;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Comments.Commands;

public record CommentDeleteRequest(string Slug, int Id) : IAuthorizationRequest;

public class CommentDeleteHandler : IAuthorizationRequestHandler<CommentDeleteRequest>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CommentDeleteHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(CommentDeleteRequest request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);
        var comment = await _context.Comments.FindAsync(
            x => x.Id == request.Id && x.ArticleId == article.Id,
            cancellationToken
        );

        if (article.AuthorId != _currentUser.User.Id && comment.AuthorId != _currentUser.User.Id)
        {
            throw new ForbiddenException();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
