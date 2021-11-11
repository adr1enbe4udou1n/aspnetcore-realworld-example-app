using Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebUI.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(context.ModelState)
            );

            return;
        }

        if (context.Exception is ValidationException)
        {
            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = context.Exception.Message,
                }
            );

            return;
        }

        if (context.Exception is UnauthorizedException)
        {
            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = context.Exception.Message,
                }
            );

            return;
        }

        if (context.Exception is ForbiddenException)
        {
            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = context.Exception.Message,
                }
            );

            return;
        }

        if (context.Exception is NotFoundException)
        {
            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = context.Exception.Message,
                }
            );

            return;
        }

        context.Result = new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = context.Exception.Message,
            }
        );
    }
}