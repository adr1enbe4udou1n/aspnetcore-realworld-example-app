using Microsoft.Extensions.Options;
using WebUI.Options;

namespace WebUI.OptionsSetup;

public class TracerOptionsSetup : IConfigureOptions<TracerOptions>
{
    private const string SectionName = "Tracing";
    private readonly IConfiguration _configuration;

    public TracerOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(TracerOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}