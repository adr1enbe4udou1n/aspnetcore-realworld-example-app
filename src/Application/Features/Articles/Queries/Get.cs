using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Articles.Queries
{
    public record ArticleEnvelope(ArticleDTO Article);

    public record ArticleGetQuery(string Slug) : IRequest<ArticleEnvelope>;

    public class ArticleGetHandler : IRequestHandler<ArticleGetQuery, ArticleEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public ArticleGetHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ArticleEnvelope> Handle(ArticleGetQuery request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

            return new ArticleEnvelope(_mapper.Map<ArticleDTO>(article));
        }
    }
}