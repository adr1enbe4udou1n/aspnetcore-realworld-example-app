using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using MediatR;

namespace Conduit.Application.Features.Profiles.Commands;

public record ProfileFollowCommand(string Username, bool Follow) : IRequest<ProfileResponse>;

public class ProfileGetHandler : IRequestHandler<ProfileFollowCommand, ProfileResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ProfileGetHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProfileResponse> Handle(ProfileFollowCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        if (request.Follow)
        {
            _context.FollowerUser.Add(new FollowerUser
            {
                Follower = _currentUser.User!,
                Following = user
            });
        }
        else
        {
            _context.FollowerUser.Remove(user.Followers
                .First(x => x.FollowerId == _currentUser.User!.Id)
            );
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileResponse(user.MapToProfile(_currentUser.User));
    }
}