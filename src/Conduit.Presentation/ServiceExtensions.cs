using System.Text.Json.Serialization;

using Conduit.Presentation.Converters;
using Conduit.Presentation.Endpoints;
using Conduit.Presentation.Exceptions;
using Conduit.Presentation.Transformers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Presentation;

public static class ServiceExtensions
{
    private const string ApiPrefix = "/api";

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddRouting(options => options.LowercaseUrls = true)
            .Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                options.SerializerOptions.Converters.Add(new DateTimeConverter());
            });

        return services
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<NotFoundExceptionHandler>()
            .AddExceptionHandler<ForbiddenExceptionHandler>()
            .AddProblemDetails()
            .AddOpenApi("openapi", o =>
            {
                o.CreateSchemaReferenceId = ConduitOpenApiSchemaReferenceId.Create;
                o.AddSchemaTransformer(new ConduitOpenApiSchemaTransformer());
                o.AddOperationTransformer(new ConduitOpenApiSecurityTransformer());
                o.AddDocumentTransformer(new ConduitOpenApiDocumentTransformer(ApiPrefix));
            });
    }

    public static void AddApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup(ApiPrefix);

        api
            .AddUserRoutes()
            .AddUsersRoutes()
            .AddProfilesRoutes()
            .AddTagsRoutes()
            .AddArticlesRoutes()
            .AddCommentsRoutes();
    }
}
