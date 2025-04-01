using System.Transactions;

using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Behaviors;

public class CommandBehavior<TRequest, TResponse>(IAppDbContext context) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (IsNotCommand(request))
        {
            context.UseRoConnection();
            return await next(cancellationToken);
        }

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var response = await next(cancellationToken);

        transactionScope.Complete();

        return response;
    }

    private static bool IsNotCommand(TRequest request)
    {
        return !request.GetType().Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
    }
}