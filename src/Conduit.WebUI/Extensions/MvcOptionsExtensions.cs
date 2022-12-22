using Conduit.WebUI.Conventions;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.WebUI.Extensions;

public static class MvcOptionsExtensions
{
    public static void UseRoutePrefix(this MvcOptions opts, string prefix)
    {
        opts.Conventions.Add(new RoutePrefixConvention(new RouteAttribute(prefix)));
    }
}