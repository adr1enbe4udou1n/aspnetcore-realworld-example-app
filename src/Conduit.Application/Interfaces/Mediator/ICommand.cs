using MediatR;

namespace Conduit.Application.Interfaces.Mediator;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}