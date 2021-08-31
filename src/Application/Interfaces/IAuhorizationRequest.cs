using MediatR;

namespace Application.Interfaces
{
    public interface IAuthorizationRequest<TRequest> : IRequest<TRequest> { }
}