using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.OpenApi;

namespace Conduit.Presentation.Transformers;

internal static class ConduitOpenApiSchemaReferenceId
{
    public static string? Create(JsonTypeInfo typeInfo)
    {
        var schemaId = OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
        if (schemaId?.EndsWith("Dto", StringComparison.Ordinal) != true)
        {
            return null;
        }

        return schemaId[..^3];
    }
}