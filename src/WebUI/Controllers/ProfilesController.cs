using Application.Features.Profiles.Commands;
using Application.Features.Profiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]/celeb_{username}")]
[ApiExplorerSettings(GroupName = "Profile")]
[Authorize]
public class ProfilesController
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Get a profile
    /// </summary>
    /// <remarks>Get a profile of a user of the system. Auth is optional</remarks>
    /// <param name="username">Username of the profile to get</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetProfileByUsername")]
    [AllowAnonymous]
    public async Task<ProfileResponse> Get(string username, CancellationToken cancellationToken)
        => await _sender.Send(new ProfileGetQuery(username), cancellationToken);

    /// <summary>
    /// Follow a user
    /// </summary>
    /// <remarks>Follow a user by username</remarks>
    /// <param name="username">Username of the profile you want to follow</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("follow", Name = "FollowUserByUsername")]
    public async Task<ProfileResponse> Follow(string username, CancellationToken cancellationToken)
        => await _sender.Send(new ProfileFollowRequest(username, true), cancellationToken);

    /// <summary>
    /// Unfollow a user
    /// </summary>
    /// <remarks>Unfollow a user by username</remarks>
    /// <param name="username">Username of the profile you want to unfollow</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("follow", Name = "UnfollowUserByUsername")]
    public async Task<ProfileResponse> Unfollow(string username, CancellationToken cancellationToken)
        => await _sender.Send(new ProfileFollowRequest(username, false), cancellationToken);
}