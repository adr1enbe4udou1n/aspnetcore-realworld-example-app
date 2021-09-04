using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Support;
using AutoMapper;

namespace Application.Features.Articles.Queries
{
    public class ArticlesFeedQuery : PagedQuery, IAuthorizationRequest<ArticlesEnvelope>
    {
    }

    public class ArticlesFeedHandler : IAuthorizationRequestHandler<ArticlesFeedQuery, ArticlesEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public ArticlesFeedHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public Task<ArticlesEnvelope> Handle(ArticlesFeedQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}