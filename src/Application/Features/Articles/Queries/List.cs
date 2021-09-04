using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using Application.Support;
using AutoMapper;
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

        public ArticlesListHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ArticlesEnvelope> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
        {
            var articles = await _context.Articles
                .Include(x => x.FavoredUsers)
                .Include(x => x.Author)
                .Include(x => x.Tags)
                .ThenInclude(x => x.Tag)
                .FilterByAuthor(request.Author)
                .FilterByTag(request.Tag)
                .FilterByFavoritedBy(request.Favorited)
                .OrderByDescending(x => x.Id)
                .PaginateAsync(request);

            return new ArticlesEnvelope(_mapper.Map<IEnumerable<ArticleDTO>>(
                articles.Items
            ), articles.Total);
        }
    }
}