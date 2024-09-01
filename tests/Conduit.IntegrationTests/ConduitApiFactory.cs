using Conduit.Infrastructure.Persistence;
using Conduit.IntegrationTests.Events;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conduit.IntegrationTests;

public class ConduitApiFactory : WebApplicationFactory<Program>
{
    public ConduitApiFactory()
    {
        using var scope = Services.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .ConfigureServices(services => services.AddLogging((builder) =>
                builder.AddProvider(new SqlCounterLoggerProvider()))
            );
    }
}