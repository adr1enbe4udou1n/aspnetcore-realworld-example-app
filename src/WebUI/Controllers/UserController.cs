using System.Threading;
using System.Threading.Tasks;
using Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.Users
{
    [Route("user")]
    [Authorize]
    public class UserController
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public Task<UserEnvelope> Register(CancellationToken cancellationToken)
            => _mediator.Send(new CurrentUserCommand(), cancellationToken);
    }
}
