using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using Conduit.Presentation.Converters;
using Conduit.Presentation.Endpoints;
using Conduit.Presentation.Exceptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Conduit.Presentation;

public static class ServiceExtensions
{
    private static readonly string[] UpdateUserNonNullableProperties = ["email", "password", "username"];
#pragma warning disable S1075
    private static readonly Uri DocumentationUri = new("https://realworld-docs.netlify.app/");
    private static readonly Uri LicenseUri = new("https://opensource.org/licenses/MIT");
#pragma warning restore S1075

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
            .AddOpenApi("v1", o =>
            {
                o.CreateSchemaReferenceId = typeInfo =>
                {
                    if (typeInfo.Type.Name.EndsWith("Request", StringComparison.Ordinal)
                        || typeInfo.Type.Name.EndsWith("Response", StringComparison.Ordinal)
                        || typeInfo.Type.Name is "ArticleSummaryDto" or "HttpValidationProblemDetails")
                    {
                        return null;
                    }

                    var schemaId = OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);

                    return schemaId?.EndsWith("Dto", StringComparison.Ordinal) == true
                        ? schemaId[..^3]
                        : schemaId;
                };

                o.AddSchemaTransformer((schema, context, cancellationToken) =>
                {
                    var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type)
                        ?? context.JsonTypeInfo.Type;

                    if (type == typeof(DateTime))
                    {
                        schema.Type = JsonSchemaType.String;
                        schema.Format = "date-time";
                    }

                    if (type == typeof(int))
                    {
                        schema.Format = null;
                    }

                    if (type.Name is "LoginUserDto" or "NewUserDto"
                        && schema.Properties?.TryGetValue("password", out var password) == true)
                    {
                        ((OpenApiSchema)password).Format = "password";
                    }

                    if (type.Name == "UpdateArticleDto" && schema.Properties is not null)
                    {
                        foreach (var property in schema.Properties.Values)
                        {
                            var propertySchema = (OpenApiSchema)property;
                            propertySchema.Type &= ~JsonSchemaType.Null;
                        }
                    }

                    if (type.Name == "UpdateUserDto" && schema.Properties is not null)
                    {
                        foreach (var propertyName in UpdateUserNonNullableProperties)
                        {
                            var propertySchema = (OpenApiSchema)schema.Properties[propertyName];
                            propertySchema.Type &= ~JsonSchemaType.Null;
                        }
                    }

                    return Task.CompletedTask;
                });

                o.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "RealWorld Conduit API";
                    document.Info.Description = "Conduit API documentation";
                    document.Info.Version = "2.0.0";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "RealWorld",
                        Url = DocumentationUri
                    };
                    document.Info.License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = LicenseUri
                    };

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

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes.TryAdd("Token", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Description = """
                            For accessing the protected API resources, you must have received a a valid JWT token after registering or logging in. This JWT token must then be used for all protected resources by passing it in via the 'Authorization' header.

                            A JWT token is generated by the API by either registering via /users or logging in via /users/login.

                            The following format must be in the 'Authorization' header :

                                Token xxxxxx.yyyyyyy.zzzzzz

                            """ + "    \n"
                    });

                    var operations = document.Paths.Values
                        .Where(path => path.Operations is not null)
                        .SelectMany(path => path.Operations!.Values);

                    if (document.Tags is not null)
                    {
                        foreach (var tag in document.Tags)
                        {
                            tag.Description = $"Everything about your {tag.Name}";
                        }
                    }

#pragma warning disable S3267, CA1861
                    var responseComponents = new Dictionary<string, IOpenApiResponse>(StringComparer.Ordinal);
                    foreach (var operation in operations)
                    {
                        if (operation.Extensions?.TryGetValue(
                            OpenApiContractExtensions.ResponseComponentExtension, out var extension) != true)
                        {
                            continue;
                        }

                        var nodeExtension = extension as JsonNodeExtension
                            ?? throw new InvalidOperationException("Invalid OpenAPI response component metadata.");
                        var metadata = (nodeExtension.Node?.GetValue<string>()
                            ?? throw new InvalidOperationException("Missing OpenAPI response component metadata."))
                            .Split(':', 2);
                        var status = metadata[0];
                        var component = metadata[1];
                        var response = operation.Responses![status];
                        responseComponents.TryAdd(component, response);
                        operation.Responses[status] = new OpenApiResponseReference(component, document);
                        operation.Extensions.Remove(OpenApiContractExtensions.ResponseComponentExtension);
                    }

                    var errorResponseComponents = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["401"] = "Unauthorized",
                        ["403"] = "Forbidden",
                        ["404"] = "NotFound",
                        ["409"] = "ConflictError",
                        ["422"] = "GenericError"
                    };
                    foreach (var errorComponent in errorResponseComponents)
                    {
                        var operation = operations.First(candidate =>
                            candidate.Responses?.ContainsKey(errorComponent.Key) == true);
                        responseComponents[errorComponent.Value] = operation.Responses![errorComponent.Key];
                    }
                    foreach (var operation in operations)
                    {
                        foreach (var errorComponent in errorResponseComponents)
                        {
                            if (operation.Responses?.ContainsKey(errorComponent.Key) == true)
                            {
                                operation.Responses[errorComponent.Key] =
                                    new OpenApiResponseReference(errorComponent.Value, document);
                            }
                        }
                    }
                    document.Components.Responses = responseComponents;

                    var requestBodyComponents = new Dictionary<string, IOpenApiRequestBody>(StringComparer.Ordinal);
                    foreach (var operation in operations)
                    {
                        if (operation.Extensions?.TryGetValue(
                            OpenApiContractExtensions.RequestBodyComponentExtension, out var extension) != true)
                        {
                            continue;
                        }

                        var nodeExtension = extension as JsonNodeExtension
                            ?? throw new InvalidOperationException("Invalid OpenAPI request body component metadata.");
                        var component = nodeExtension.Node?.GetValue<string>()
                            ?? throw new InvalidOperationException("Missing OpenAPI request body component metadata.");
                        var requestBody = operation.RequestBody!;
                        requestBodyComponents[component] = requestBody;
                        operation.RequestBody = new OpenApiRequestBodyReference(component, document);
                        operation.Extensions.Remove(OpenApiContractExtensions.RequestBodyComponentExtension);
                    }
                    document.Components.RequestBodies = requestBodyComponents;

                    var parameterComponents = new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);
                    foreach (var operation in operations)
                    {
                        if (operation.Parameters is null)
                        {
                            continue;
                        }
                        for (var index = 0; index < operation.Parameters.Count; index++)
                        {
                            var parameter = operation.Parameters[index];
                            if (parameter.Extensions?.TryGetValue(
                                OpenApiContractExtensions.ParameterComponentExtension, out var extension) == true)
                            {
                                var nodeExtension = extension as JsonNodeExtension
                                    ?? throw new InvalidOperationException("Invalid OpenAPI parameter component metadata.");
                                var componentName = nodeExtension.Node?.GetValue<string>()
                                    ?? throw new InvalidOperationException("Missing OpenAPI parameter component metadata.");
                                parameter.Extensions.Remove(OpenApiContractExtensions.ParameterComponentExtension);
                                parameterComponents.TryAdd(componentName, parameter);
                                operation.Parameters[index] =
                                    new OpenApiParameterReference(componentName, document);
                            }
                        }
                    }
                    document.Components.Parameters = parameterComponents;

                    var errorSchema = (OpenApiSchema)responseComponents["GenericError"]
                        .Content!["application/json"].Schema!;
                    document.Components.Schemas!["GenericErrorModel"] = errorSchema;
#pragma warning restore S3267, CA1861
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