using System.Text.Json;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Presentation.Exceptions;

public class ValidationExceptionHandler(
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var failures = validationException.Errors.ToArray();
        var invalidCredentials = failures.Length == 0;
        var statusCode = StatusCodes.Status422UnprocessableEntity;
        if (invalidCredentials)
        {
            statusCode = StatusCodes.Status401Unauthorized;
        }
        else if (failures.Any(e => e.ErrorCode == "Conflict"))
        {
            statusCode = StatusCodes.Status409Conflict;
        }

        httpContext.Response.StatusCode = statusCode;
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Detail = "One or more validation errors occurred",
                Status = statusCode
            }
        };

        var errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => JsonNamingPolicy.CamelCase.ConvertName(g.Key),
                g => g.Select(e => e.ErrorCode is "NotEmptyValidator" or "NotNullValidator"
                    ? "can't be blank"
                    : e.ErrorMessage).ToArray()
            );
        if (invalidCredentials)
        {
            errors.Add("credentials", ["invalid"]);
        }
        context.ProblemDetails.Extensions.Add("errors", errors);

        return await problemDetailsService.TryWriteAsync(context);
    }
}
