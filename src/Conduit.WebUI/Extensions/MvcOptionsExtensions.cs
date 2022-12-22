using Microsoft.AspNetCore.Mvc;
using Conduit.WebUI.Conventions;

namespace Conduit.WebUI.Extensions;

public static class MvcOptionsExtensions
{
    public static void UseRoutePrefix(this MvcOptions opts, string prefix)
    {
        opts.Conventions.Add(new RoutePrefixConvention(new RouteAttribute(prefix)));
    }
}