using Conduit.Application.Exceptions;

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
            Results.StatusCode(StatusCodes.Status404NotFound);
            return Results.Problem(ex.Message);
        }
        catch (ValidationException ex)
        {
            Results.StatusCode(StatusCodes.Status400BadRequest);
            return Results.ValidationProblem(ex.Errors, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            Results.StatusCode(StatusCodes.Status403Forbidden);
            return Results.Problem(ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            Results.StatusCode(StatusCodes.Status401Unauthorized);
            return Results.Problem(ex.Message);
        }
    }
}