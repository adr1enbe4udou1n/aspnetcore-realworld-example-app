using Microsoft.OpenApi;

namespace Conduit.Presentation.Extensions;

internal static class OpenApiDocumentExtensions
{
    public static void MovePathPrefixToServer(this OpenApiDocument document, string prefix)
    {
        var normalizedPrefix = $"/{prefix.Trim('/')}";
        document.Servers = [new OpenApiServer { Url = normalizedPrefix }];

        var paths = new OpenApiPaths();
        foreach (var path in document.Paths)
        {
            var relativePath = path.Key.StartsWith($"{normalizedPrefix}/", StringComparison.OrdinalIgnoreCase)
                ? path.Key[normalizedPrefix.Length..]
                : path.Key;
            paths.Add(relativePath, path.Value);
        }
        document.Paths = paths;
    }
}