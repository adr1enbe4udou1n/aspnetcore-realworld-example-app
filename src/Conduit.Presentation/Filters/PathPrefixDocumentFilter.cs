using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.Filters;

public class PathPrefixDocumentFilter : IDocumentFilter
{
    private readonly string _pathPrefix;

    public PathPrefixDocumentFilter(string pathPrefix)
    {
        _pathPrefix = pathPrefix;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.ToDictionary(
            entry => entry.Key.Replace($"/{_pathPrefix}", string.Empty, StringComparison.InvariantCultureIgnoreCase),
            entry => entry.Value
        );

        swaggerDoc.Paths.Clear();

        foreach (var (key, value) in paths)
        {
            swaggerDoc.Paths.Add(key, value);
        }
    }
}