using System.Threading;
using System.Threading.Tasks;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.Users
{
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "User and Authentication")]
    public class UsersController
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <remarks>Register a new user</remarks>
        /// <param name="command">Details of the new user to register</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<UserEnvelope> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
            => _mediator.Send(command, cancellationToken);

        /// <summary>
        /// Existing user login
        /// </summary>
        /// <remarks>Login for existing user</remarks>
        /// <param name="command">Credentials to use</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public Task<UserEnvelope> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
            => _mediator.Send(command, cancellationToken);
    }
}
