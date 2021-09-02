using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Interfaces;

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
        public Task<ArticleEnvelope> Handle(ArticleCreateCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}