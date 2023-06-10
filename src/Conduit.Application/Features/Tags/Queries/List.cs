using Conduit.Application.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Tags.Queries;

public record TagsResponse(IEnumerable<string> Tags);

#pragma warning disable S2094
public record TagsListQuery : IRequest<TagsResponse>;
#pragma warning restore S2094

public class TagsListHandler : IRequestHandler<TagsListQuery, TagsResponse>
{
    private readonly IAppDbContext _context;

    public TagsListHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TagsResponse> Handle(TagsListQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return new TagsResponse(tags.Select(t => t.Name));
    }
}