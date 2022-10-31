using MediatR;

namespace Application.Interfaces.Mediator;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}