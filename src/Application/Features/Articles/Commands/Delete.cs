using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Articles.Commands
{
    public record ArticleDeleteCommand(string Slug) : IAuthorizationRequest;

    public class ArticleDeleteHandler : IAuthorizationRequestHandler<ArticleDeleteCommand>
    {
        private readonly IAppDbContext _context;

        public ArticleDeleteHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ArticleDeleteCommand request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}