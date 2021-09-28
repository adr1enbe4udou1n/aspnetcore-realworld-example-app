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

        [HttpGet]
        public async Task<ProfileEnvelope> Get(string username, CancellationToken cancellationToken)
            => await _mediator.Send(new ProfileGetQuery(username), cancellationToken);

        [HttpPost("follow")]
        [Authorize]
        public async Task<ProfileEnvelope> Follow(string username, CancellationToken cancellationToken)
            => await _mediator.Send(new ProfileFollowCommand(username, true), cancellationToken);

        [HttpDelete("follow")]
        [Authorize]
        public async Task<ProfileEnvelope> Unfollow(string username, CancellationToken cancellationToken)
            => await _mediator.Send(new ProfileFollowCommand(username, false), cancellationToken);
    }
}
