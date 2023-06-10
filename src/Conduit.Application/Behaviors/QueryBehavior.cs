using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Behaviors;

public class QueryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IAppDbContext _context;

    public QueryBehavior(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (IsQuery(request))
        {
            _context.UseRoConnection();
        }

        return await next();
    }

    private static bool IsQuery(TRequest request)
    {
        return request.GetType().Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
    }
}