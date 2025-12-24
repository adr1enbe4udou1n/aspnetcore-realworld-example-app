using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Presentation.Endpoints;

public static class ArticlesEndpoints
{
    public static IEndpointRouteBuilder AddArticlesRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/articles", (IQueryArticles articles,
            string? author,
            string? favorited,
            string? tag,
            int? limit,
            int? offset,
            CancellationToken cancellationToken) =>
            articles.List(new ArticlesListQuery
            {
                Author = author,
                Favorited = favorited,
                Tag = tag,
                Limit = limit,
                Offset = offset
            }, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticles")
            .WithSummary("Get recent articles globally")
            .WithDescription("Get most recent articles globally. Use query parameters to filter results. Auth is optional")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Filter by author (username)";
                parameter = operation.Parameters![1];
                parameter.Description = "Filter by favorites of a user (username)";
                parameter = operation.Parameters![2];
                parameter.Description = "Filter by tag";
                parameter = operation.Parameters![3];
                parameter.Description = "Limit number of articles returned (default is 20)";
                parameter = operation.Parameters![4];
                parameter.Description = "Offset/skip number of articles (default is 0)";
                return Task.CompletedTask;
            });

        app.MapGet("/articles/feed", (IQueryArticles articles,
            int? limit,
            int? offset,
            CancellationToken cancellationToken) =>
            articles.Feed(new ArticlesFeedQuery
            {
                Limit = limit,
                Offset = offset
            }, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticlesFeed")
            .WithSummary("Get recent articles from users you follow")
            .WithDescription("Get most recent articles from users you follow. Use query parameters to limit. Auth is required")
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Limit number of articles returned (default is 20)";
                parameter = operation.Parameters![1];
                parameter.Description = "Offset/skip number of articles (default is 0)";
                return Task.CompletedTask;
            });

        app.MapGet("/articles/{slug}", (IQueryArticles articles, string slug, CancellationToken cancellationToken) =>
            articles.Find(slug, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticle")
            .WithSummary("Get an article")
            .WithDescription("Get an article. Auth not required")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article to get";
                return Task.CompletedTask;
            });

        app.MapPost("/articles", (ICommandArticles articles, NewArticleRequest request, CancellationToken cancellationToken) =>
            articles.Create(request.Article, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("CreateArticle")
            .WithSummary("Create an article")
            .WithDescription("Create an article. Auth is required")
            .Produces(200)
            .ProducesValidationProblem(400)
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.RequestBody!.Description = "Article to create";
                return Task.CompletedTask;
            });

        app.MapPut("/articles/{slug}", (ICommandArticles articles, string slug, UpdateArticleRequest request, CancellationToken cancellationToken) =>
            articles.Update(slug, request.Article, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("UpdateArticle")
            .WithSummary("Update an article")
            .WithDescription("Update an article. Auth is required")
            .Produces(200)
            .ProducesValidationProblem(400)
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article to update";
                operation.RequestBody!.Description = "Article to update";
                return Task.CompletedTask;
            });

        app.MapDelete("/articles/{slug}", (ICommandArticles articles, string slug, CancellationToken cancellationToken) =>
            articles.Delete(slug, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("DeleteArticle")
            .WithSummary("Delete an article")
            .WithDescription("Delete an article. Auth is required")
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article to delete";
                return Task.CompletedTask;
            });

        app.MapPost("/articles/{slug}/favorite", (ICommandArticles articles, string slug, CancellationToken cancellationToken) =>
            articles.Favorite(slug, true, cancellationToken)
        )
            .WithTags("Favorites")
            .WithName("CreateArticleFavorite")
            .WithSummary("Favorite an article")
            .WithDescription("Favorite an article. Auth is required")
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to favorite";
                return Task.CompletedTask;
            });

        app.MapDelete("/articles/{slug}/favorite", (ICommandArticles articles, string slug, CancellationToken cancellationToken) =>
            articles.Favorite(slug, false, cancellationToken)
        )
            .WithTags("Favorites")
            .WithName("DeleteArticleFavorite")
            .WithSummary("Unfavorite an article")
            .WithDescription("Unfavorite an article. Auth is required")
            .RequireAuthorization()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article that you want to unfavorite";
                return Task.CompletedTask;
            });

        return app;
    }
}

public record NewArticleRequest(NewArticleDto Article);
public record UpdateArticleRequest(UpdateArticleDto Article);