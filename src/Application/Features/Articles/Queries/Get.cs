using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Articles.Queries
{
    public record ArticleEnvelope(ArticleDTO Article);

    public record ArticleGetQuery(string Slug) : IRequest<ArticleEnvelope>;

    public class ArticleGetHandler : IRequestHandler<ArticleGetQuery, ArticleEnvelope>
    {
        private readonly IAppDbContext _context;

        public ArticleGetHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<ArticleEnvelope> Handle(ArticleGetQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}