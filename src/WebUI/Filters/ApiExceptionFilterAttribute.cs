using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebUI.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException)
            {
                var exception = context.Exception as ValidationException;

                context.Result = new BadRequestObjectResult(
                    new ValidationProblemDetails(
                        exception.Errors
                            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray())
                    )
                );
            }
        }
    }
}
