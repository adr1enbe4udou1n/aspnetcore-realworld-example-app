using Conduit.Application.Features.Profiles.Commands;
using Conduit.Application.Features.Profiles.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Presentation.Controllers;

[Route("[controller]/{username}")]
[ApiExplorerSettings(GroupName = "Profile")]
[Authorize]
public class ProfilesController
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get a profile
    /// </summary>
    /// <remarks>Get a profile of a user of the system. Auth is optional</remarks>
    /// <param name="username">Username of the profile to get</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetProfileByUsername")]
    [AllowAnonymous]
    public Task<ProfileResponse> Get(string username, CancellationToken cancellationToken)
    {
        return _sender.Send(new ProfileGetQuery(username), cancellationToken);
    }

    /// <summary>
    /// Follow a user
    /// </summary>
    /// <remarks>Follow a user by username</remarks>
    /// <param name="username">Username of the profile you want to follow</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("follow", Name = "FollowUserByUsername")]
    public Task<ProfileResponse> Follow(string username, CancellationToken cancellationToken)
    {
        return _sender.Send(new ProfileFollowCommand(username, true), cancellationToken);
    }

    /// <summary>
    /// Unfollow a user
    /// </summary>
    /// <remarks>Unfollow a user by username</remarks>
    /// <param name="username">Username of the profile you want to unfollow</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("follow", Name = "UnfollowUserByUsername")]
    public Task<ProfileResponse> Unfollow(string username, CancellationToken cancellationToken)
    {
        return _sender.Send(new ProfileFollowCommand(username, false), cancellationToken);
    }
}