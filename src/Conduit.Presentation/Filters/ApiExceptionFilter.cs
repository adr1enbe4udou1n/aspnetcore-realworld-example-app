using Conduit.Application.Exceptions;

using FluentValidation;

using Microsoft.AspNetCore.Http;

namespace Conduit.Presentation.Filters;

public sealed class ApiExceptionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next
    )
    {
        try
        {
            return await next(context);
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key.ToUpperInvariant(),
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return Results.ValidationProblem(
                errors, ex.Message, statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (UnauthorizedException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status401Unauthorized);
        }
    }
}