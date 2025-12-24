using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class CommentsApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IOpenApiParameter parameter;

        if (operation.OperationId == "GetArticleComments")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article that you want to get comments for";
        }

        if (operation.OperationId == "CreateArticleComment")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article that you want to create a comments for";
        }

        if (operation.OperationId == "DeleteArticleComment")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article that you want to delete a comments for";
            parameter = operation.Parameters![1];
            parameter.Description = "ID of the comment you want to delete";
        }
    }
}
