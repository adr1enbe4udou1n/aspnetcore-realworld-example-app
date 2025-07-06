namespace Conduit.Application.Features.Auth.Queries;

public interface IQueryUsers
{
    Task<UserResponse> Find(CancellationToken cancellationToken);
}