using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.EndPoints;

public static class ArticlesEndpoints
{
    public static void AddArticlesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/articles", (ISender sender, [AsParameters] ArticlesListQuery query, CancellationToken cancellationToken) =>
            sender.Send(query, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticles")
            .WithSummary("Get recent articles globally")
            .WithDescription("Get most recent articles globally. Use query parameters to filter results. Auth is optional")
            .WithOpenApi();

        app.MapGet("/articles/feed", (ISender sender, [AsParameters] ArticlesFeedQuery query, CancellationToken cancellationToken) =>
            sender.Send(query, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticlesFeed")
            .WithSummary("Get recent articles from users you follow")
            .WithDescription("Get most recent articles from users you follow. Use query parameters to limit. Auth is required")
            .RequireAuthorization()
            .WithOpenApi();

        app.MapGet("/articles/{slug}", (ISender sender, string slug, CancellationToken cancellationToken) =>
            sender.Send(new ArticleGetQuery(slug), cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticle")
            .WithSummary("Get an article")
            .WithDescription("Get an article. Auth not required")
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article to get";
                return generatedOperation;
            });

        app.MapPost("/articles", (ISender sender, NewArticleRequest request, CancellationToken cancellationToken) =>
            sender.Send(new NewArticleCommand(request.Article), cancellationToken)
        )
            .WithTags("Articles")
            .WithName("CreateArticle")
            .WithSummary("Create an article")
            .WithDescription("Create an article. Auth is required")
            .Produces(200)
            .ProducesProblem(400)
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                generatedOperation.RequestBody.Description = "Article to create";
                return generatedOperation;
            });

        app.MapPut("/articles/{slug}", (ISender sender, string slug, UpdateArticleRequest request, CancellationToken cancellationToken) =>
            sender.Send(new UpdateArticleCommand(slug, request.Article), cancellationToken)
        )
            .WithTags("Articles")
            .WithName("UpdateArticle")
            .WithSummary("Update an article")
            .WithDescription("Update an article. Auth is required")
            .Produces(200)
            .ProducesProblem(400)
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article to update";
                generatedOperation.RequestBody.Description = "Article to update";
                return generatedOperation;
            });

        app.MapDelete("/articles/{slug}", (ISender sender, string slug, CancellationToken cancellationToken) =>
            sender.Send(new ArticleDeleteCommand(slug), cancellationToken)
        )
            .WithTags("Articles")
            .WithName("DeleteArticle")
            .WithSummary("Delete an article")
            .WithDescription("Delete an article. Auth is required")
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article to delete";
                return generatedOperation;
            });

        app.MapPost("/articles/{slug}/favorite", (ISender sender, string slug, CancellationToken cancellationToken) =>
            sender.Send(new ArticleFavoriteCommand(slug, true), cancellationToken)
        )
            .WithTags("Favorites")
            .WithName("CreateArticleFavorite")
            .WithSummary("Favorite an article")
            .WithDescription("Favorite an article. Auth is required")
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article that you want to favorite";
                return generatedOperation;
            });

        app.MapDelete("/articles/{slug}/favorite", (ISender sender, string slug, CancellationToken cancellationToken) =>
            sender.Send(new ArticleFavoriteCommand(slug, false), cancellationToken)
        )
            .WithTags("Favorites")
            .WithName("DeleteArticleFavorite")
            .WithSummary("Unfavorite an article")
            .WithDescription("Unfavorite an article. Auth is required")
            .RequireAuthorization()
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Slug of the article that you want to unfavorite";
                return generatedOperation;
            });
    }
}

public record NewArticleRequest(NewArticleDto Article);
public record UpdateArticleRequest(UpdateArticleDto Article);