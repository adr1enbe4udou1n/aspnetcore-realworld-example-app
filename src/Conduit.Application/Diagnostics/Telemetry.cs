using System.Diagnostics;

namespace Conduit.Application.Diagnostics;

public static class Telemetry
{
    public static readonly ActivitySource ApplicationActivitySource = new("ASPNET Core RealWorld");
}