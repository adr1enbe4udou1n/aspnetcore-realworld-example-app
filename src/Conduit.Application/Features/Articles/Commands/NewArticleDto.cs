using System.Collections.ObjectModel;

namespace Conduit.Application.Features.Articles.Commands;

public class NewArticleDto
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string Body { get; set; }

#pragma warning disable CA2227
    public Collection<string> TagList { get; set; } = [];
#pragma warning restore CA2227
}
