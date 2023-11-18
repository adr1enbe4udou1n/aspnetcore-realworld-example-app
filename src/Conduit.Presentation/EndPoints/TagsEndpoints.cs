using Conduit.Application.Features.Tags.Queries;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.EndPoints;

public static class TagsEndpoints
{
    public static void AddTagsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", (ISender sender, CancellationToken cancellationToken) =>
            sender.Send(new TagsListQuery(), cancellationToken)
        )
            .WithTags("Tags")
            .WithName("GetTags")
            .WithSummary("Get tags")
            .WithDescription("Get tags. Auth not required")
            .WithOpenApi();
    }
}