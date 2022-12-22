using MediatR;

namespace Application.Interfaces.Mediator;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}