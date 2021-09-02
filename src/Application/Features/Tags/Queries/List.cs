using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Application.Features.Tags.Queries
{
    public record TagsEnvelope(IEnumerable<string> Tags);

    public record TagsListQuery() : IRequest<TagsEnvelope>;

    public class TagsListHandler : IRequestHandler<TagsListQuery, TagsEnvelope>
    {
        public Task<TagsEnvelope> Handle(TagsListQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}