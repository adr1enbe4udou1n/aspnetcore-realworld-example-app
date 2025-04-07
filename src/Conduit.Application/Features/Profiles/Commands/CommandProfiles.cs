using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Interfaces;

namespace Conduit.Application.Features.Profiles.Commands;

public class CommandProfiles(IAppDbContext context, ICurrentUser currentUser) : ICommandProfiles
{
    public async Task<ProfileResponse> Follow(string username, bool follow, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FindAsync(x => x.Name == username, cancellationToken);

        if (follow)
        {
            user.AddFollower(currentUser.User!);
        }
        else
        {
            user.RemoveFollower(currentUser.User!);
        }

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return new ProfileResponse(user.MapToProfile(currentUser.User));
    }
}