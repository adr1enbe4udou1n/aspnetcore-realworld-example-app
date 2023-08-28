using System.Transactions;

using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Behaviors;

public class CommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IAppDbContext _context;

    public CommandBehavior(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (IsNotCommand(request))
        {
            _context.UseRoConnection();
            return await next();
        }

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var response = await next();

        transactionScope.Complete();

        return response;
    }

    private static bool IsNotCommand(TRequest request)
    {
        return !request.GetType().Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
    }
}