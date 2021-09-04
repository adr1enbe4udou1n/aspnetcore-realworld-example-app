using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using Application.Support;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ArticlesEnvelope> Handle(ArticlesFeedQuery request, CancellationToken cancellationToken)
        {
            var articles = await _context.Articles
                .Include(x => x.FavoredUsers)
                .Include(x => x.Author)
                .Include(x => x.Tags)
                .ThenInclude(x => x.Tag)
                .HasAuthorsFollowedBy(_currentUser.User)
                .OrderByDescending(x => x.Id)
                .PaginateAsync(request);

            return new ArticlesEnvelope(_mapper.Map<IEnumerable<ArticleDTO>>(
                articles.Items
            ), articles.Total);
        }
    }
}