using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Support;
using MediatR;

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

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public IEnumerable<string> TagList { get; set; }

        public AuthorDTO Author { get; set; }

        public bool Favorited { get; set; }

        public int FavoritesCount { get; set; }
    }

    public class ArticlesEnvelope
    {
        public IEnumerable<ArticleDTO> Articles { get; set; }

        public int ArticlesCount { get; set; }
    }

    public class ArticlesListQuery : PagedQuery, IRequest<ArticlesEnvelope>
    {
        public string Author { get; set; }

        public string Favorited { get; set; }

        public string Tag { get; set; }
    }

    public class ArticlesListHandler : IRequestHandler<ArticlesListQuery, ArticlesEnvelope>
    {
        private readonly IAppDbContext _context;

        public ArticlesListHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<ArticlesEnvelope> Handle(ArticlesListQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}