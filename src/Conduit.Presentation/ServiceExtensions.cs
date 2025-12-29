using Conduit.Presentation.Converters;
using Conduit.Presentation.Endpoints;
using Conduit.Presentation.Exceptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Conduit.Presentation;

public static class ServiceExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddRouting(options => options.LowercaseUrls = true)
            .Configure<JsonOptions>(options =>
                options.SerializerOptions.Converters.Add(new DateTimeConverter())
            );

        return services
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<NotFoundExceptionHandler>()
            .AddExceptionHandler<ForbiddenExceptionHandler>()
            .AddProblemDetails()
            .AddOpenApi("swagger", o =>
            {
                o.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Servers =
                    [
                        new() {
                            Url = "/api"
                        }
                    ];

                    var newPaths = new OpenApiPaths();
                    foreach (var path in document.Paths)
                    {
                        var newPathKey = path.Key.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ? path.Key[4..] : path.Key;
                        newPaths.Add(newPathKey, path.Value);
                    }
                    document.Paths = newPaths;
                    return Task.CompletedTask;
                });
            });
    }

    public static void AddApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api
            .AddUserRoutes()
            .AddUsersRoutes()
            .AddProfilesRoutes()
            .AddTagsRoutes()
            .AddArticlesRoutes()
            .AddCommentsRoutes();
    }
}