using Conduit.Application.Features.Profiles.Queries;

namespace Conduit.Application.Features.Profiles.Commands;

public interface ICommandProfiles
{
    Task<ProfileResponse> Follow(string username, bool follow, CancellationToken cancellationToken);
}