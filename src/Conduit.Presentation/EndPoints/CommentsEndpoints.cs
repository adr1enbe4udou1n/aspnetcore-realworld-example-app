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
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to get comments for";
                return Task.CompletedTask;
            });

        app.MapPost("/articles/{slug}/comments", (ICommandComments comments, string slug, NewCommentRequest request, CancellationToken cancellationToken) =>
            comments.Create(slug, request.Comment, cancellationToken)
        )
            .WithTags("Comments")
            .WithName("CreateArticleComment")
            .WithSummary("Create a comment for an article")
            .WithDescription("Create a comment for an article. Auth is required")
            .Produces(200)
            .ProducesValidationProblem(400)
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to create a comments for";
                return Task.CompletedTask;
            });

        app.MapDelete("/articles/{slug}/comments/{commentId}", (ICommandComments comments, string slug, int commentId, CancellationToken cancellationToken) =>
            comments.Delete(slug, commentId, cancellationToken)
        )
            .WithTags("Comments")
            .WithName("DeleteArticleComment")
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required")
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to delete a comments for";
                parameter = operation.Parameters![1];
                parameter.Description = "ID of the comment you want to delete";
                return Task.CompletedTask;
            });

        return app;
    }
}

public record NewCommentRequest(NewCommentDto Comment);