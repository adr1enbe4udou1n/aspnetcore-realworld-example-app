using Application.Interfaces;
using MediatR;

namespace Application.Behaviors;

public class DbTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IAppDbContext _context;

    public DbTransactionBehavior(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        TResponse result;

        using var _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            result = await next();

            await _transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return result;
    }
}
