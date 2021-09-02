using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Application.Features.Articles.Queries
{
    public record ArticleEnvelope(ArticleDTO Article);

    public record ArticleGetQuery(string slug) : IRequest<ArticleEnvelope>;

    public class ArticleGetHandler : IRequestHandler<ArticleGetQuery, ArticleEnvelope>
    {
        public Task<ArticleEnvelope> Handle(ArticleGetQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}