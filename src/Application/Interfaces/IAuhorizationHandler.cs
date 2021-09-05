using MediatR;

namespace Application.Interfaces
{
    public interface IAuthorizationRequestHandler<TRequest> : IRequestHandler<TRequest>
        where TRequest : IAuthorizationRequest
    { }

    public interface IAuthorizationRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IAuthorizationRequest<TResponse>
    { }
}
