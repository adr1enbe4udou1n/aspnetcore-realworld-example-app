using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Queries;

public record TagsResponse(IEnumerable<string> Tags);

public record TagsListQuery() : IRequest<TagsResponse>;

public class TagsListHandler : IRequestHandler<TagsListQuery, TagsResponse>
{
    private readonly IAppDbContext _context;

    public TagsListHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TagsResponse> Handle(TagsListQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Tags.OrderBy(t => t.Name).AsNoTracking().ToListAsync(cancellationToken);
        return new TagsResponse(tags.Select(t => t.Name));
    }
}