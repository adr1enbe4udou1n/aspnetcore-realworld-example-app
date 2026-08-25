using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Conduit.Presentation;

internal static class OpenApiContractExtensions
{
    public static RouteHandlerBuilder WithOpenApiResponse(
        this RouteHandlerBuilder builder,
        int statusCode,
        string componentName,
        string description)
    {
        return builder.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            var status = statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var response = operation.Responses![status];
            response.Description = description;

            var document = context.Document
                ?? throw new InvalidOperationException("Missing OpenAPI document.");
            var components = document.Components ??= new OpenApiComponents();
            var responses = components.Responses ??=
                new Dictionary<string, IOpenApiResponse>(StringComparer.Ordinal);
            responses.TryAdd(componentName, response);
            operation.Responses[status] = new OpenApiResponseReference(componentName, document);
            return Task.CompletedTask;
        });
    }

    public static RouteHandlerBuilder WithOpenApiErrors(
        this RouteHandlerBuilder builder,
        params int[] statusCodes)
    {
        return builder.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            operation.Responses!.Remove(StatusCodes.Status400BadRequest.ToString(
                System.Globalization.CultureInfo.InvariantCulture));

            foreach (var statusCode in statusCodes)
            {
                var status = statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture);
                operation.Responses.TryAdd(status, CreateErrorResponse(statusCode));
            }
            return Task.CompletedTask;
        });
    }

    public static RouteHandlerBuilder WithOpenApiRequestBody(
        this RouteHandlerBuilder builder,
        string componentName,
        string codegenName)
    {
        return builder.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            var requestBody = operation.RequestBody!;
            var document = context.Document
                ?? throw new InvalidOperationException("Missing OpenAPI document.");
            var components = document.Components ??= new OpenApiComponents();
            var requestBodies = components.RequestBodies ??=
                new Dictionary<string, IOpenApiRequestBody>(StringComparer.Ordinal);
            requestBodies.TryAdd(componentName, requestBody);
            operation.RequestBody = new OpenApiRequestBodyReference(componentName, document);

            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions["x-codegen-request-body-name"] =
                new JsonNodeExtension(JsonValue.Create(codegenName));
            return Task.CompletedTask;
        });
    }

    public static RouteHandlerBuilder WithTokenSecurity(this RouteHandlerBuilder builder)
    {
        return builder.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Token", context.Document)] = []
                }
            ];
            return Task.CompletedTask;
        });
    }

    public static RouteHandlerBuilder WithOpenApiParameter(
        this RouteHandlerBuilder builder,
        string parameterName,
        string componentName)
    {
        return builder.AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
        {
            var parameters = operation.Parameters!;
            var parameter = parameters.Single(candidate => candidate.Name == parameterName);

            var document = context.Document
                ?? throw new InvalidOperationException("Missing OpenAPI document.");
            var components = document.Components ??= new OpenApiComponents();
            var componentParameters = components.Parameters ??=
                new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);
            componentParameters.TryAdd(componentName, parameter);
            parameters[parameters.IndexOf(parameter)] =
                new OpenApiParameterReference(componentName, document);
            return Task.CompletedTask;
        });
    }

    private static OpenApiResponse CreateErrorResponse(int statusCode)
    {
        return new()
        {
            Description = statusCode switch
            {
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden. The error key identifies the resource type (article, comment, etc.)",
                StatusCodes.Status404NotFound => "Not Found. The error key identifies the resource type (article, profile, comment, etc.)",
                StatusCodes.Status409Conflict => "Conflict - resource already exists",
                _ => "Unexpected error"
            },
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Example = JsonNode.Parse(statusCode switch
                    {
                        StatusCodes.Status401Unauthorized => """{"errors":{"token":["is missing"]}}""",
                        StatusCodes.Status403Forbidden => """{"errors":{"resource":["forbidden"]}}""",
                        StatusCodes.Status404NotFound => """{"errors":{"resource":["not found"]}}""",
                        StatusCodes.Status409Conflict => """{"errors":{"username":["has already been taken"]}}""",
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
        };
    }
}