using System.Text.Json.Serialization;

using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;

namespace Conduit.Application.Features.Profiles.Queries;

public class ProfileDto
{
    public required string Username { get; set; }

    [JsonRequired]
    public string? Bio { get; set; }

    [JsonRequired]
    public string? Image { get; set; }

    [JsonRequired]
    public bool Following { get; set; }
}

public record ProfileResponse(ProfileDto Profile);

public class QueryProfiles(IAppDbContext context, ICurrentUser currentUser) : IQueryProfiles
{
    public async Task<ProfileResponse> Find(string username, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FindAsync(x => x.Name == username, "profile", cancellationToken);

        return new ProfileResponse(user.MapToProfile(currentUser.User));
    }
}
