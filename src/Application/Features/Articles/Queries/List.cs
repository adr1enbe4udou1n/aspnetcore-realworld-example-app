using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using Application.Support;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Queries
{
    public class AuthorDTO
    {

        public string Username { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }
    }

    public class ArticleDTO
    {
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public IEnumerable<string> TagList { get; set; }

        public AuthorDTO Author { get; set; }

        public bool Favorited { get; set; }

        public int FavoritesCount { get; set; }
    }

    public record ArticlesEnvelope(IEnumerable<ArticleDTO> Articles, int ArticlesCount);

    public class ArticlesListQuery : PagedQuery, IRequest<ArticlesEnvelope>
    {
        public string Author { get; set; }

        public string Favorited { get; set; }

        public string Tag { get; set; }
    }

    public class ArticlesListHandler : IRequestHandler<ArticlesListQuery, ArticlesEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public ArticlesListHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ArticlesEnvelope> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
        {
            var articles = await _context.Articles
                .FilterByAuthor(request.Author)
                .FilterByTag(request.Tag)
                .FilterByFavoritedBy(request.Favorited)
                .OrderByDescending(x => x.Id)
                .ProjectTo<ArticleDTO>(_mapper.ConfigurationProvider, new
                {
                    currentUser = _currentUser.User
                })
                .PaginateAsync(request, cancellationToken);

            return new ArticlesEnvelope(articles.Items, articles.Total);
        }
    }
}
