using Conduit.Application.Features.Profiles.Commands;
using Conduit.Application.Features.Profiles.Queries;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder AddProfilesRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{username}", (IQueryProfiles profiles, string username, CancellationToken cancellationToken) =>
            profiles.Find(username, cancellationToken)
        )
            .WithTags("Profile")
            .WithName("GetProfileByUsername")
            .WithSummary("Get a profile")
            .WithDescription("Get a profile of a user of the system. Auth is optional")
            .WithOpenApi();

        app.MapPost("/profiles/{username}/follow", (ICommandProfiles profiles, string username, CancellationToken cancellationToken) =>
            profiles.Follow(username, true, cancellationToken)
        )
            .WithTags("Profile")
            .WithName("FollowUserByUsername")
            .WithSummary("Follow a user")
            .WithDescription("Follow a user by username")
            .WithOpenApi()
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Username of the profile you want to follow";
                return generatedOperation;
            });

        app.MapDelete("/profiles/{username}/follow", (ICommandProfiles profiles, string username, CancellationToken cancellationToken) =>
            profiles.Follow(username, false, cancellationToken)
        )
            .WithTags("Profile")
            .WithName("UnfollowUserByUsername")
            .WithSummary("Unfollow a user")
            .WithDescription("Unfollow a user by username")
            .WithOpenApi()
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Username of the profile you want to unfollow";
                return generatedOperation;
            });

        return app;
    }
}