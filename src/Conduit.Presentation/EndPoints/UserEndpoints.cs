using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder AddUserRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/user", (IQueryUsers users, CancellationToken cancellationToken) =>
            users.Find(cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("GetCurrentUser")
            .WithSummary("Get current user")
            .WithDescription("Gets the currently logged-in user")
            .RequireAuthorization();

        app.MapPut("/user", (ICommandUsers users, UpdateUserRequest request, CancellationToken cancellationToken) =>
            users.Update(request.User, cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("UpdateCurrentUser")
            .WithSummary("Update current user")
            .WithDescription("Updated user information for current user")
            .Produces(200)
            .ProducesValidationProblem(400)
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.RequestBody!.Description = "User details to update. At least <strong>one</strong> field is required.";
                return Task.CompletedTask;
            });

        return app;
    }
}

public record UpdateUserRequest(UpdateUserDto User);