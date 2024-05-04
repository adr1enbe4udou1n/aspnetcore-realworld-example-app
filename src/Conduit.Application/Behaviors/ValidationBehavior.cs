using FluentValidation;

using MediatR;

using ValidationException = Conduit.Application.Exceptions.ValidationException;

namespace Conduit.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validator != null)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResult = await validator.ValidateAsync(context, cancellationToken);

            if (validationResult.Errors.Count > 0)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        return await next();
    }
}