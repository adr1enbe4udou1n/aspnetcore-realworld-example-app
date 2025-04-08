using Conduit.Application.Support;

using MediatR;

namespace Conduit.Application.Features.Articles.Queries;

public class ArticlesFeedQuery : PagedQuery, IRequest<MultipleArticlesResponse>
{
}
