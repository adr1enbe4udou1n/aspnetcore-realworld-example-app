using Conduit.Application.Features.Comments.Commands;
using Conduit.Application.Features.Comments.Queries;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.EndPoints;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder AddCommentsRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{slug}/comments", (ISender sender, string slug, CancellationToken cancellationToken) =>
            sender.Send(new CommentsListQuery(slug), cancellationToken)
        )
            .WithTags("Comments")
            .WithName("GetArticleComments")
            .WithSummary("Get comments for an article")
            .WithDescription("Get the comments for an article. Auth is optional")
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article that you want to get comments for";
                return generatedOperation;
            });

        app.MapPost("/articles/{slug}/comments", (ISender sender, string slug, NewCommentRequest request, CancellationToken cancellationToken) =>
            sender.Send(new NewCommentCommand(slug, request.Comment), cancellationToken)
        )
            .WithTags("Comments")
            .WithName("CreateArticleComment")
            .WithSummary("Create a comment for an article")
            .WithDescription("Create a comment for an article. Auth is required")
            .Produces(200)
            .ProducesValidationProblem(400)
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article that you want to create a comments for";
                return generatedOperation;
            });

        app.MapDelete("/articles/{slug}/comments/{commentId}", (ISender sender, string slug, int commentId, CancellationToken cancellationToken) =>
            sender.Send(new CommentDeleteCommand(slug, commentId), cancellationToken)
        )
            .WithTags("Comments")
            .WithName("DeleteArticleComment")
            .WithSummary("Delete a comment for an article")
            .WithDescription("Delete a comment for an article. Auth is required")
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article that you want to delete a comments for";
                parameter = generatedOperation.Parameters[1];
                parameter.Description = "ID of the comment you want to delete";
                return generatedOperation;
            });

        return app;
    }
}

public record NewCommentRequest(NewCommentDto Comment);