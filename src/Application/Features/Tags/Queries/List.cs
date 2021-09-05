using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Queries
{
    public record TagsEnvelope(IEnumerable<string> Tags);

    public record TagsListQuery() : IRequest<TagsEnvelope>;

    public class TagsListHandler : IRequestHandler<TagsListQuery, TagsEnvelope>
    {
        private readonly IAppDbContext _context;

        public TagsListHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<TagsEnvelope> Handle(TagsListQuery request, CancellationToken cancellationToken)
        {
            var tags = await _context.Tags.OrderBy(t => t.Name).AsNoTracking().ToListAsync(cancellationToken);
            return new TagsEnvelope(tags.Select(t => t.Name));
        }
    }
}
