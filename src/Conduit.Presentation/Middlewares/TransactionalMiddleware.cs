using System.Transactions;

using Conduit.Application.Interfaces;

using Microsoft.AspNetCore.Http;

namespace Conduit.Presentation.Middlewares;

public class TransactionalMiddleware(
    RequestDelegate next
)
{
    public async Task InvokeAsync(HttpContext ctx, IAppDbContext dbContext)
    {
        var method = ctx.Request.Method;

        if (method is "GET" or "HEAD")
        {
            dbContext.UseRoConnection();
            await next(ctx);
            return;
        }

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await next(ctx);

        transactionScope.Complete();
    }
}