using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

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

                    var successResponses = new Dictionary<string, (string Status, string Description)>(StringComparer.Ordinal)
                    {
                        ["GetArticles"] = ("200", "Multiple articles"),
                        ["CreateArticle"] = ("201", "Single article"),
                        ["GetArticlesFeed"] = ("200", "Multiple articles"),
                        ["DeleteArticle"] = ("204", "No content"),
                        ["GetArticle"] = ("200", "Single article"),
                        ["UpdateArticle"] = ("200", "Single article"),
                        ["GetArticleComments"] = ("200", "Multiple comments"),
                        ["CreateArticleComment"] = ("201", "Single comment"),
                        ["DeleteArticleComment"] = ("204", "No content"),
                        ["DeleteArticleFavorite"] = ("200", "Single article"),
                        ["CreateArticleFavorite"] = ("200", "Single article"),
                        ["GetProfileByUsername"] = ("200", "Profile"),
                        ["UnfollowUserByUsername"] = ("200", "Profile"),
                        ["FollowUserByUsername"] = ("200", "Profile"),
                        ["GetTags"] = ("200", "Tags"),
                        ["GetCurrentUser"] = ("200", "User"),
                        ["UpdateCurrentUser"] = ("200", "User"),
                        ["CreateUser"] = ("201", "User"),
                        ["Login"] = ("200", "User")
                    };

                    var errorResponses = new Dictionary<string, string[]>(StringComparer.Ordinal)
                    {
                        ["GetArticles"] = ["401", "422"],
                        ["CreateArticle"] = ["401", "409", "422"],
                        ["GetArticlesFeed"] = ["401", "422"],
                        ["DeleteArticle"] = ["401", "403", "404", "422"],
                        ["GetArticle"] = ["404", "422"],
                        ["UpdateArticle"] = ["401", "403", "404", "422"],
                        ["GetArticleComments"] = ["401", "404", "422"],
                        ["CreateArticleComment"] = ["401", "404", "422"],
                        ["DeleteArticleComment"] = ["401", "403", "404", "422"],
                        ["DeleteArticleFavorite"] = ["401", "404", "422"],
                        ["CreateArticleFavorite"] = ["401", "404", "422"],
                        ["GetProfileByUsername"] = ["401", "404", "422"],
                        ["UnfollowUserByUsername"] = ["401", "404", "422"],
                        ["FollowUserByUsername"] = ["401", "404", "422"],
                        ["GetTags"] = ["422"],
                        ["GetCurrentUser"] = ["401", "422"],
                        ["UpdateCurrentUser"] = ["401", "422"],
                        ["CreateUser"] = ["409", "422"],
                        ["Login"] = ["401", "422"]
                    };

                    var securedOperations = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "CreateArticle", "GetArticlesFeed", "DeleteArticle", "UpdateArticle",
                        "CreateArticleComment", "DeleteArticleComment", "DeleteArticleFavorite",
                        "CreateArticleFavorite", "UnfollowUserByUsername", "FollowUserByUsername",
                        "GetCurrentUser", "UpdateCurrentUser"
                    };

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

                            """
                    });

                    var operations = document.Paths.Values
                        .Where(path => path.Operations is not null)
                        .SelectMany(path => path.Operations!.Values);

                    foreach (var operation in operations)
                    {
                        if (operation.OperationId is null)
                        {
                            continue;
                        }

                        if (successResponses.TryGetValue(operation.OperationId, out var success)
                            && operation.Responses?.TryGetValue(success.Status, out var response) == true)
                        {
                            response.Description = success.Description;
                        }

                        operation.Responses?.Remove("400");
                        if (operation.Responses is not null
                            && errorResponses.TryGetValue(operation.OperationId, out var statuses))
                        {
                            foreach (var status in statuses)
                            {
                                operation.Responses.TryAdd(status, new OpenApiResponse
                                {
                                    Description = status switch
                                    {
                                        "401" => "Unauthorized",
                                        "403" => "Forbidden. The error key identifies the resource type (article, comment, etc.)",
                                        "404" => "Not Found. The error key identifies the resource type (article, profile, comment, etc.)",
                                        "409" => "Conflict - resource already exists",
                                        _ => "Unexpected error"
                                    },
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new()
                                        {
                                            Example = JsonNode.Parse(status switch
                                            {
                                                "401" => """{"errors":{"token":["is missing"]}}""",
                                                "403" => """{"errors":{"resource":["forbidden"]}}""",
                                                "404" => """{"errors":{"resource":["not found"]}}""",
                                                "409" => """{"errors":{"username":["has already been taken"]}}""",
                                                _ => """{"errors":{"title":["can't be blank"]}}"""
                                            }),
                                            Schema = new OpenApiSchema
                                            {
                                                Type = JsonSchemaType.Object,
                                                Required = new HashSet<string>(StringComparer.Ordinal) { "errors" },
                                                Properties = new Dictionary<string, IOpenApiSchema>
                                                {
                                                    ["errors"] = new OpenApiSchema
                                                    {
                                                        Type = JsonSchemaType.Object,
                                                        AdditionalProperties = new OpenApiSchema
                                                        {
                                                            Type = JsonSchemaType.Array,
                                                            Items = new OpenApiSchema { Type = JsonSchemaType.String }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                });
                            }
                        }

                        if (securedOperations.Contains(operation.OperationId))
                        {
                            operation.Security =
                            [
                                new OpenApiSecurityRequirement
                                {
                                    [new OpenApiSecuritySchemeReference("Token", document)] = []
                                }
                            ];
                        }

                        var requestBodyNames = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["CreateArticle"] = "article",
                            ["UpdateArticle"] = "article",
                            ["CreateArticleComment"] = "comment",
                            ["UpdateCurrentUser"] = "body",
                            ["CreateUser"] = "body",
                            ["Login"] = "body"
                        };
                        if (requestBodyNames.TryGetValue(operation.OperationId, out var requestBodyName))
                        {
                            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                            operation.Extensions["x-codegen-request-body-name"] =
                                new JsonNodeExtension(JsonValue.Create(requestBodyName));
                        }
                    }
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
