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

namespace Application.Features.Comments.Commands
{
    public record CommentDeleteCommand(string Slug, int Id) : IAuthorizationRequest;

    public class CommentDeleteHandler : IAuthorizationRequestHandler<CommentDeleteCommand>
    {
        private readonly IAppDbContext _context;

        public CommentDeleteHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CommentDeleteCommand request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);
            var comment = await _context.Comments.FindAsync(
                x => x.Id == request.Id && x.ArticleId == article.Id,
                cancellationToken
            );

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}