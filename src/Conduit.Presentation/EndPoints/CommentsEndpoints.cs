using Conduit.Application.Features.Comments.Commands;
using Conduit.Application.Features.Comments.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder AddCommentsRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{slug}/comments", (IQueryComments comments, string slug, CancellationToken cancellationToken) =>
            comments.List(slug, cancellationToken)
        )
            .WithTags("Comments")
            .WithName("GetArticleComments")
            .WithSummary("Get comments for an article")
            .WithDescription("Get the comments for an article. Auth is optional")
            .WithOpenApiResponse(200, "MultipleCommentsResponse", "Multiple comments")
            .WithOpenApiErrors(401, 404, 422)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to get comments for";
                return Task.CompletedTask;
            });

        app.MapPost("/articles/{slug}/comments", async (ICommandComments comments, string slug, NewCommentRequest request, CancellationToken cancellationToken) =>
        {
            var response = await comments.Create(slug, request.Comment, cancellationToken);
            return Results.Created((string?)null, response);
        })
            .WithTags("Comments")
            .WithName("CreateArticleComment")
            .WithSummary("Create a comment for an article")
            .WithDescription("Create a comment for an article. Auth is required")
            .Produces<SingleCommentResponse>(StatusCodes.Status201Created)
            .RequireAuthorization()
            .WithOpenApiResponse(201, "SingleCommentResponse", "Single comment")
            .WithOpenApiErrors(401, 404, 422)
            .WithTokenSecurity()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to create a comment for";
                operation.RequestBody!.Description = "Comment you want to create";
                return Task.CompletedTask;
            })
            .WithOpenApiRequestBody("NewCommentRequest", "comment");

        app.MapDelete("/articles/{slug}/comments/{id}", async (ICommandComments comments, string slug, int id, CancellationToken cancellationToken) =>
        {
            await comments.Delete(slug, id, cancellationToken);
            return Results.NoContent();
        }
        )
            .WithTags("Comments")
            .WithName("DeleteArticleComment")
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization()
            .WithOpenApiResponse(204, "EmptyOkResponse", "No content")
            .WithOpenApiErrors(401, 403, 404, 422)
            .WithTokenSecurity()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to delete a comment for";
                parameter = operation.Parameters[1];
                parameter.Description = "ID of the comment you want to delete";
                ((Microsoft.OpenApi.OpenApiSchema)parameter.Schema!).Format = null;
                return Task.CompletedTask;
            });

        return app;
    }
}

public record NewCommentRequest(NewCommentDto Comment);