using Conduit.Application.Features.Tags.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder AddTagsRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags", ([FromServices] IQueryTags tags, CancellationToken cancellationToken) =>
            tags.List(cancellationToken)
        )
            .WithTags("Tags")
            .WithName("GetTags")
            .WithSummary("Get tags")
            .WithDescription("Get tags. Auth not required");

        return app;
    }
}