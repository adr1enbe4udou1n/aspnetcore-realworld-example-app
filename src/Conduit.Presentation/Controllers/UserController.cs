using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Presentation.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(GroupName = "User and Authentication")]
[Authorize]
public class UserController
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get current user
    /// </summary>
    /// <remarks>Gets the currently logged-in user</remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetCurrentUser")]
    public Task<UserResponse> Current(CancellationToken cancellationToken)
    {
        return _sender.Send(new CurrentUserQuery(), cancellationToken);
    }

    /// <summary>
    /// Update current user
    /// </summary>
    /// <remarks>Updated user information for current user</remarks>
    /// <param name="request">User details to update. At least <strong>one</strong> field is required.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut(Name = "UpdateCurrentUser")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public Task<UserResponse> Update([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return _sender.Send(new UpdateUserCommand(request.User), cancellationToken);
    }
}

public record UpdateUserRequest(UpdateUserDto User);