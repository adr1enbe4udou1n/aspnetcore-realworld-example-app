using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class UserApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.OperationId == "UpdateCurrentUser")
        {
            operation.RequestBody!.Description = "User details to update. At least <strong>one</strong> field is required.";
        }
    }
}
