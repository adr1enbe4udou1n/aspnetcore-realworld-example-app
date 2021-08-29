using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebUI.Filters
{
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
        }
    }
}
