using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder AddUsersRoutes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (ICommandUsers users, NewUserRequest request, CancellationToken cancellationToken) =>
        {
            var response = await users.Register(request.User, cancellationToken);
            return Results.Created((string?)null, response);
        })
            .WithTags("User and Authentication")
            .WithName("CreateUser")
            .WithDescription("Register a new user")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(400)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.RequestBody!.Description = "Details of the new user to register";
                return Task.CompletedTask;
            });

        app.MapPost("/users/login", (ICommandUsers users, LoginUserRequest request, CancellationToken cancellationToken) =>
            users.Login(request.User, cancellationToken)
        )
            .WithTags("User and Authentication")
            .WithName("Login")
            .WithSummary("Existing user login")
            .WithDescription("Login for existing user")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(400)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.RequestBody!.Description = "Credentials to use";
                return Task.CompletedTask;
            });

        return app;
    }
}

public record LoginUserRequest(LoginUserDto User);
public record NewUserRequest(NewUserDto User);