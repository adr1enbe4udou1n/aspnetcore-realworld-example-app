using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class ProfilesApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IOpenApiParameter parameter;

        if (operation.OperationId == "FollowUserByUsername")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Username of the profile you want to follow";
        }

        if (operation.OperationId == "UnfollowUserByUsername")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Username of the profile you want to unfollow";
        }
    }
}
