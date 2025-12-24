using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class ArticlesApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IOpenApiParameter parameter;

        if (operation.OperationId == "GetArticles")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Filter by author (username)";
            parameter = operation.Parameters![1];
            parameter.Description = "Filter by favorites of a user (username)";
            parameter = operation.Parameters![2];
            parameter.Description = "Filter by tag";
            parameter = operation.Parameters![3];
            parameter.Description = "Limit number of articles returned (default is 20)";
            parameter = operation.Parameters![4];
            parameter.Description = "Offset/skip number of articles (default is 0)";
        }

        if (operation.OperationId == "GetArticlesFeed")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Limit number of articles returned (default is 20)";
            parameter = operation.Parameters![1];
            parameter.Description = "Offset/skip number of articles (default is 0)";
        }

        if (operation.OperationId == "GetArticle")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article to get";
        }

        if (operation.OperationId == "CreateArticle")
        {
            operation.RequestBody!.Description = "Article to create";
        }

        if (operation.OperationId == "UpdateArticle")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article to update";
            operation.RequestBody!.Description = "Article to update";
        }

        if (operation.OperationId == "DeleteArticle")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article to delete";
        }

        if (operation.OperationId == "CreateArticleFavorite")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article that you want to favorite";
        }

        if (operation.OperationId == "DeleteArticleFavorite")
        {
            parameter = operation.Parameters![0];
            parameter.Description = "Slug of the article that you want to unfavorite";
        }
    }
}
