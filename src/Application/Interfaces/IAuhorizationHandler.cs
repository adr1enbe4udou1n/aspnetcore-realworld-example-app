using MediatR;

namespace Application.Interfaces
{
    public interface IAuthorizationRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IAuthorizationRequest<TResponse>
    { }
}