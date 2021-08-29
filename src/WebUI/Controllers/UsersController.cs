using System.Threading;
using System.Threading.Tasks;
using Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.Users
{
    [Route("users")]
    public class UsersController
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<UserEnvelope> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            return _mediator.Send(command, cancellationToken);
        }

        [HttpPost("login")]
        public Task<UserEnvelope> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            return _mediator.Send(command, cancellationToken);
        }
    }
}
