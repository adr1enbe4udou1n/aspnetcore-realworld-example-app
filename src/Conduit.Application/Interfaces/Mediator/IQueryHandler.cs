using MediatR;

namespace Conduit.Application.Interfaces.Mediator;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}