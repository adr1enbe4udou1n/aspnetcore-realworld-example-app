using Microsoft.AspNetCore.Mvc;
using WebUI.Conventions;

namespace WebUI.Extensions;

public static class MvcOptionsExtensions
{
    public static void UseRoutePrefix(this MvcOptions opts, string prefix)
    {
        opts.Conventions.Add(new RoutePrefixConvention(new RouteAttribute(prefix)));
    }
}