using Conduit.Application.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Tags.Queries;

public record TagsResponse(IEnumerable<string> Tags);

#pragma warning disable S2094
public record TagsListQuery : IRequest<TagsResponse>;
#pragma warning restore S2094

public class TagsListHandler(IAppDbContext context) : IRequestHandler<TagsListQuery, TagsResponse>
{
    public async Task<TagsResponse> Handle(TagsListQuery request, CancellationToken cancellationToken)
    {
        var tags = await context.Tags.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return new TagsResponse(tags.Select(t => t.Name));
    }
}