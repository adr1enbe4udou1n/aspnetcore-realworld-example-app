using FluentValidation;

using MediatR;

using ValidationException = Conduit.Application.Exceptions.ValidationException;

namespace Conduit.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validator != null)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResult = await _validator.ValidateAsync(context, cancellationToken);

            if (validationResult.Errors.Any())
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        return await next();
    }
}