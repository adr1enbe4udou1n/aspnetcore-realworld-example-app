using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Articles.Commands
{
    public class ArticleCreateDTO
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public IEnumerable<string> TagList { get; set; }
    }

    public record ArticleCreateCommand(ArticleCreateDTO Article) : IAuthorizationRequest<ArticleEnvelope>;

    public class ArticleCreateHandler : IAuthorizationRequestHandler<ArticleCreateCommand, ArticleEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public ArticleCreateHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ArticleEnvelope> Handle(ArticleCreateCommand request, CancellationToken cancellationToken)
        {
            var article = _mapper.Map<Article>(request.Article);
            await _context.Articles.AddAsync(article, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new ArticleEnvelope(_mapper.Map<ArticleDTO>(article));
        }
    }
}