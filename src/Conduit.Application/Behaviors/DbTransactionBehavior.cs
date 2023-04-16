using Conduit.Application.Interfaces;
using Conduit.Application.Interfaces.Mediator;

using MediatR;

namespace Conduit.Application.Behaviors;

public class DbTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IAppDbContext _context;

    public DbTransactionBehavior(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IQuery<TResponse>)
        {
            return await next();
        }

        return await _context.UseTransactionAsync(next, cancellationToken);
    }
}