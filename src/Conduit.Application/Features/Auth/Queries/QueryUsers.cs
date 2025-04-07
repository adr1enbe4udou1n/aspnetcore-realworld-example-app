using Conduit.Application.Interfaces;

namespace Conduit.Application.Features.Auth.Queries;

public record UserResponse(UserDto User);

public class QueryUsers(ICurrentUser currentUser, IJwtTokenGenerator jwtTokenGenerator) : IQueryUsers
{
    public Task<UserResponse> Find(CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserResponse(
            currentUser.User!.Map(jwtTokenGenerator)
        ));
    }
}