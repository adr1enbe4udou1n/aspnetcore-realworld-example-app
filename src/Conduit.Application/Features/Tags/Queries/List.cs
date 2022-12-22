using Application.Interfaces;
using Application.Interfaces.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Queries;

public record TagsResponse(IEnumerable<string> Tags);

public record TagsListQuery() : IQuery<TagsResponse>;

public class TagsListHandler : IQueryHandler<TagsListQuery, TagsResponse>
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