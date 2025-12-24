using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class UsersApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.OperationId == "CreateUser")
        {
            operation.RequestBody!.Description = "Details of the new user to register";
        }

        if (operation.OperationId == "Login")
        {
            operation.RequestBody!.Description = "Credentials to use";
        }
    }
}
