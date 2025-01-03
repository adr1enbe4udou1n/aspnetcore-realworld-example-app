using Conduit.Application.Features.Tags.Queries;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder AddTagsRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", (ISender sender, CancellationToken cancellationToken) =>
            sender.Send(new TagsListQuery(), cancellationToken)
        )
            .WithTags("Tags")
            .WithName("GetTags")
            .WithSummary("Get tags")
            .WithDescription("Get tags. Auth not required")
            .WithOpenApi();

        return app;
    }
}