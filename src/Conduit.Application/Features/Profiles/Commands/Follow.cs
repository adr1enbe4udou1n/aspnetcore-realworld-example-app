using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using MediatR;

namespace Conduit.Application.Features.Profiles.Commands;

public record ProfileFollowCommand(string Username, bool Follow) : IRequest<ProfileResponse>;

public class ProfileGetHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<ProfileFollowCommand, ProfileResponse>
{
    public async Task<ProfileResponse> Handle(ProfileFollowCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        if (request.Follow)
        {
            context.FollowerUser.Add(new FollowerUser
            {
                Follower = currentUser.User!,
                Following = user
            });
        }
        else
        {
            context.FollowerUser.Remove(user.Followers
                .First(x => x.FollowerId == currentUser.User!.Id)
            );
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ProfileResponse(user.MapToProfile(currentUser.User));
    }
}