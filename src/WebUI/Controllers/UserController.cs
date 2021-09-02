using System.Threading;
using System.Threading.Tasks;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.Users
{
    [Route("user")]
    public class UserController
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public Task<UserEnvelope> Current(CancellationToken cancellationToken)
            => _mediator.Send(new CurrentUserQuery(), cancellationToken);

        [HttpPut]
        public Task<UserEnvelope> Update([FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
            => _mediator.Send(command, cancellationToken);
    }
}
