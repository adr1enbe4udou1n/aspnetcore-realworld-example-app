using Conduit.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Tags.Queries;

public record TagsResponse(IEnumerable<string> Tags);

public class QueryTags(IAppDbContext context) : IQueryTags
{
    public async Task<TagsResponse> List(CancellationToken cancellationToken)
    {
        var tags = await context.Tags.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return new TagsResponse(tags.Select(t => t.Name));
    }
}