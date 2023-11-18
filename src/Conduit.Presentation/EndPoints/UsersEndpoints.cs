using Conduit.Application.Features.Auth.Commands;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.EndPoints;

public static class UsersEndpoints
{
    public static void AddUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users", (ISender sender, NewUserRequest request, CancellationToken cancellationToken) =>
            sender.Send(new NewUserCommand(request.User), cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("CreateUser")
            .WithSummary("Register a new user")
            .WithDescription("Register a new user")
            .WithOpenApi(generatedOperation =>
            {
                generatedOperation.RequestBody.Description = "Details of the new user to register";
                return generatedOperation;
            });

        app.MapPost("/users/login", (ISender sender, LoginUserRequest request, CancellationToken cancellationToken) =>
            sender.Send(new LoginUserCommand(request.User), cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("Login")
            .WithSummary("Existing user login")
            .WithDescription("Login for existing user")
            .Produces(200)
            .ProducesProblem(400)
            .WithOpenApi(generatedOperation =>
            {
                generatedOperation.RequestBody.Description = "Credentials to use";
                return generatedOperation;
            });
    }
}

public record LoginUserRequest(LoginUserDto User);
public record NewUserRequest(NewUserDto User);