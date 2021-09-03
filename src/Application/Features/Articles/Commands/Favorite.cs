using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Interfaces;

namespace Application.Features.Articles.Commands
{
    public record ArticleFavoriteCommand(string Slug, bool Favorite) : IAuthorizationRequest<ArticleEnvelope>;

    public class ArticleFavoriteHandler : IAuthorizationRequestHandler<ArticleFavoriteCommand, ArticleEnvelope>
    {
        private readonly IAppDbContext _context;

        public ArticleFavoriteHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<ArticleEnvelope> Handle(ArticleFavoriteCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}