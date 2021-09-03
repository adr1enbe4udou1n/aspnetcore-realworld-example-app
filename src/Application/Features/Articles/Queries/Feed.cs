using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Support;

namespace Application.Features.Articles.Queries
{
    public class ArticlesFeedQuery : PagedQuery, IAuthorizationRequest<ArticlesEnvelope>
    {
    }

    public class ArticlesFeedHandler : IAuthorizationRequestHandler<ArticlesFeedQuery, ArticlesEnvelope>
    {
        private readonly IAppDbContext _context;

        public ArticlesFeedHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<ArticlesEnvelope> Handle(ArticlesFeedQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}