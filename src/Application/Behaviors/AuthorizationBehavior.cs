using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Interfaces;
using MediatR;

namespace Application.Behaviors
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ICurrentUser _currentUser;

        public AuthorizationBehavior(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request is IAuthorizationRequest || request is IAuthorizationRequest<TResponse>)
            {
                if (!_currentUser.IsAuthenticated)
                {
                    throw new UnauthorizedException();
                }
            }

            return await next();
        }
    }
}
