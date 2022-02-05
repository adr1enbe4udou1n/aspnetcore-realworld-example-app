using System.Diagnostics;

namespace Application.Diagnostics;

public static class Telemetry
{
    public static readonly ActivitySource ApplicationActivitySource = new("ASPNET Core RealWorld");
}