
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class PathPrefixDocumentFilter(string pathPrefix) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.ToDictionary(
            entry => entry.Key.Replace($"/{pathPrefix}", string.Empty, StringComparison.InvariantCultureIgnoreCase),
            entry => entry.Value
        );

        swaggerDoc.Paths.Clear();

        foreach (var (key, value) in paths)
        {
            swaggerDoc.Paths.Add(key, value);
        }
    }
}