using Application.Interfaces;
using Application.Interfaces.Mediator;
using MediatR;

namespace Application.Behaviors;

public class QueryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IAppDbContext _context;

    public QueryBehavior(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IQuery<TResponse>)
        {
            _context.UseRoConnection();
        }

        return await next();
    }
}