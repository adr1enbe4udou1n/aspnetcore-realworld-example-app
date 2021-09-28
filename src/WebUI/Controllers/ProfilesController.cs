using System.Threading;
using System.Threading.Tasks;
using Application.Features.Profiles.Commands;
using Application.Features.Profiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("[controller]/celeb_{username}")]
    [ApiExplorerSettings(GroupName = "Profile")]
    public class ProfilesController
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Get a profile
        /// </summary>
        /// <remarks>Get a profile of a user of the system. Auth is optional</remarks>
        /// <param name="username"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ProfileEnvelope> Get(string username, CancellationToken cancellationToken)
            => await _mediator.Send(new ProfileGetQuery(username), cancellationToken);

        /// <summary>
        /// Follow a user
        /// </summary>
        /// <remarks>Follow a user by username</remarks>
        /// <param name="username"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("follow")]
        [Authorize]
        public async Task<ProfileEnvelope> Follow(string username, CancellationToken cancellationToken)
            => await _mediator.Send(new ProfileFollowCommand(username, true), cancellationToken);

        /// <summary>
        /// Unfollow a user
        /// </summary>
        /// <remarks>Unfollow a user by username</remarks>
        /// <param name="username"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("follow")]
        [Authorize]
        public async Task<ProfileEnvelope> Unfollow(string username, CancellationToken cancellationToken)
            => await _mediator.Send(new ProfileFollowCommand(username, false), cancellationToken);
    }
}
