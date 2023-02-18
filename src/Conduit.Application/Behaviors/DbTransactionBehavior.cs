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
        TResponse result;

        using var _transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            result = await next();

            _transaction.Commit();
        }
        catch (Exception)
        {
            _transaction.Rollback();
            throw;
        }

        return result;
    }
}
