using MediatR;

namespace Application.Interfaces.Mediator;

public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
