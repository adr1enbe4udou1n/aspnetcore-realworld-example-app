using Conduit.Application.Interfaces;
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
        return await _context.UseTransactionAsync(next, cancellationToken);
    }
}