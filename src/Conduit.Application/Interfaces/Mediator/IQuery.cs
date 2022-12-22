using MediatR;

namespace Conduit.Application.Interfaces.Mediator;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}