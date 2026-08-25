using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Presentation.Extensions;

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
            .WithOpenApiResponse(200, "MultipleArticlesResponse", "Multiple articles")
            .WithOpenApiErrors(401, 422)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Filter by author (username)";
                parameter = operation.Parameters[1];
                parameter.Description = "Filter by favorites of a user (username)";
                parameter = operation.Parameters[2];
                parameter.Description = "Filter by tag";
                parameter = operation.Parameters[3];
                parameter.Description = "The numbers of items to return.";
                var schema = (Microsoft.OpenApi.OpenApiSchema)parameter.Schema!;
                schema.Format = null;
                schema.Minimum = "1";
                schema.Default = 20;
                parameter = operation.Parameters[4];
                parameter.Description = "The number of items to skip before starting to collect the result set.";
                schema = (Microsoft.OpenApi.OpenApiSchema)parameter.Schema!;
                schema.Format = null;
                schema.Minimum = "0";
                return Task.CompletedTask;
            })
            .WithOpenApiParameter("limit", "limitParam")
            .WithOpenApiParameter("offset", "offsetParam");

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
            .WithOpenApiResponse(200, "MultipleArticlesResponse", "Multiple articles")
            .WithOpenApiErrors(401, 422)
            .WithTokenSecurity()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "The numbers of items to return.";
                var schema = (Microsoft.OpenApi.OpenApiSchema)parameter.Schema!;
                schema.Format = null;
                schema.Minimum = "1";
                schema.Default = 20;
                parameter = operation.Parameters[1];
                parameter.Description = "The number of items to skip before starting to collect the result set.";
                schema = (Microsoft.OpenApi.OpenApiSchema)parameter.Schema!;
                schema.Format = null;
                schema.Minimum = "0";
                return Task.CompletedTask;
            })
            .WithOpenApiParameter("limit", "limitParam")
            .WithOpenApiParameter("offset", "offsetParam");

        app.MapGet("/articles/{slug}", (IQueryArticles articles, string slug, CancellationToken cancellationToken) =>
            articles.Find(slug, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("GetArticle")
            .WithSummary("Get an article")
            .WithDescription("Get an article. Auth not required")
            .WithOpenApiResponse(200, "SingleArticleResponse", "Single article")
            .WithOpenApiErrors(404, 422)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article to get";
                return Task.CompletedTask;
            });

        app.MapPost("/articles", async (ICommandArticles articles, NewArticleRequest request, CancellationToken cancellationToken) =>
        {
            var response = await articles.Create(request.Article, cancellationToken);
            return Results.Created((string?)null, response);
        })
            .WithTags("Articles")
            .WithName("CreateArticle")
            .WithSummary("Create an article")
            .WithDescription("Create an article. Auth is required")
            .Produces<SingleArticleResponse>(StatusCodes.Status201Created)
            .RequireAuthorization()
            .WithOpenApiResponse(201, "SingleArticleResponse", "Single article")
            .WithOpenApiErrors(401, 409, 422)
            .WithTokenSecurity()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.RequestBody!.Description = "Article to create";
                return Task.CompletedTask;
            })
            .WithOpenApiRequestBody("NewArticleRequest", "article");

        app.MapPut("/articles/{slug}", (ICommandArticles articles, string slug, UpdateArticleRequest request, CancellationToken cancellationToken) =>
            articles.Update(slug, request.Article, cancellationToken)
        )
            .WithTags("Articles")
            .WithName("UpdateArticle")
            .WithSummary("Update an article")
            .WithDescription("Update an article. Auth is required")
            .Produces<SingleArticleResponse>(StatusCodes.Status200OK)
            .RequireAuthorization()
            .WithOpenApiResponse(200, "SingleArticleResponse", "Single article")
            .WithOpenApiErrors(401, 403, 404, 422)
            .WithTokenSecurity()
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var parameter = operation.Parameters![0];
                parameter.Description = "Slug of the article to update";
                operation.RequestBody!.Description = "Article to update";
                return Task.CompletedTask;
            })
            .WithOpenApiRequestBody("UpdateArticleRequest", "article");

        app.MapDelete("/articles/{slug}", async (ICommandArticles articles, string slug, CancellationToken cancellationToken) =>
        {
            await articles.Delete(slug, cancellationToken);
            return Results.NoContent();
        }
        )
            .WithTags("Articles")
            .WithName("DeleteArticle")
            .WithSummary("Delete an article")
            .WithDescription("Delete an article. Auth is required")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization()
            .WithOpenApiResponse(204, "EmptyOkResponse", "No content")
            .WithOpenApiErrors(401, 403, 404, 422)
            .WithTokenSecurity()
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
            .WithOpenApiResponse(200, "SingleArticleResponse", "Single article")
            .WithOpenApiErrors(401, 404, 422)
            .WithTokenSecurity()
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
            .WithOpenApiResponse(200, "SingleArticleResponse", "Single article")
            .WithOpenApiErrors(401, 404, 422)
            .WithTokenSecurity()
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