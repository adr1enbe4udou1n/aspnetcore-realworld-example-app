using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Conduit.Presentation;

internal sealed class ConduitOpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type)
            ?? context.JsonTypeInfo.Type;

        if (type == typeof(DateTime))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "date-time";
        }
        else if (type == typeof(int))
        {
            schema.Format = null;
        }

        var attributeProvider = context.JsonPropertyInfo?.AttributeProvider;
        if (attributeProvider is null)
        {
            return Task.CompletedTask;
        }

        var dataType = attributeProvider
            .GetCustomAttributes(typeof(DataTypeAttribute), true)
            .OfType<DataTypeAttribute>()
            .FirstOrDefault();
        if (dataType?.DataType == DataType.Password)
        {
            schema.Format = "password";
        }

        var disallowsNull = attributeProvider
            .GetCustomAttributes(typeof(DisallowNullAttribute), true).Length > 0;
        if (!disallowsNull && attributeProvider is PropertyInfo { SetMethod: not null } propertyInfo)
        {
            disallowsNull = propertyInfo.SetMethod.GetParameters()[0]
                .IsDefined(typeof(DisallowNullAttribute), true);
        }
        if (disallowsNull)
        {
            schema.Type &= ~JsonSchemaType.Null;
        }

        return Task.CompletedTask;
    }
}