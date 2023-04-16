using Conduit.WebUI.Options;

using Microsoft.Extensions.Options;

using Npgsql;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Conduit.WebUI.OptionsSetup;

public class TracerProviderBuilderSetup : IPostConfigureOptions<TracerProviderBuilder>
{
    private readonly TracerOptions _tracerOptions;

    public TracerProviderBuilderSetup(IOptions<TracerOptions> tracerOptions)
    {
        _tracerOptions = tracerOptions.Value;
    }

    public void PostConfigure(string? name, TracerProviderBuilder options)
    {
        if (!_tracerOptions.Enabled)
        {
            return;
        }

        options.SetResourceBuilder(ResourceBuilder
                .CreateDefault()
                .AddService("ASPNET Core RealWorld"))
            .AddAspNetCoreInstrumentation()
            .AddNpgsql()
            .AddSource("ASPNET Core RealWorld")
            .AddJaegerExporter(o =>
            {
                o.AgentHost = _tracerOptions.Host;
                o.AgentPort = _tracerOptions.Port;
            });
    }
}