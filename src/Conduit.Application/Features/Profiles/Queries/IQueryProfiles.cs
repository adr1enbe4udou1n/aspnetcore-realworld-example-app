namespace Conduit.Application.Features.Profiles.Queries;

public interface IQueryProfiles
{
    Task<ProfileResponse> Find(string username, CancellationToken cancellationToken);
}
