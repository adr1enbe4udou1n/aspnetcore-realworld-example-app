using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Articles.Commands
{
    public record ArticleDeleteRequest(string Slug) : IAuthorizationRequest;

    public class ArticleDeleteHandler : IAuthorizationRequestHandler<ArticleDeleteRequest>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;

        public ArticleDeleteHandler(IAppDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(ArticleDeleteRequest request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

            if (article.AuthorId != _currentUser.User.Id)
            {
                throw new ForbiddenException();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
