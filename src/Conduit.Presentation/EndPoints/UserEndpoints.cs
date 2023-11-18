using Carter;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.EndPoints;

public class UserEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user", (ISender sender, CancellationToken cancellationToken) =>
            sender.Send(new CurrentUserQuery(), cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("GetCurrentUser")
            .WithSummary("Get current user")
            .WithDescription("Gets the currently logged-in user")
            .RequireAuthorization()
            .WithOpenApi();

        app.MapPut("/user", (ISender sender, UpdateUserRequest request, CancellationToken cancellationToken) =>
            sender.Send(new UpdateUserCommand(request.User), cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("UpdateCurrentUser")
            .WithSummary("Update current user")
            .WithDescription("Updated user information for current user")
            .Produces(200)
            .ProducesProblem(400)
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                generatedOperation.RequestBody.Description = "User details to update. At least <strong>one</strong> field is required.";
                return generatedOperation;
            });
    }
}

public record UpdateUserRequest(UpdateUserDto User);