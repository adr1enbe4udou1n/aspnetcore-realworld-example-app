using Application.IntegrationTests.Events;
using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.IntegrationTests;

public class ConduitApiFactory : WebApplicationFactory<Program>
{
    public ConduitApiFactory()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5434;User Id=main;Password=main;Database=main;";

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "super secret key");

        var scope = Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        context.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLogging((builder) => builder.AddProvider(new SqlCounterLoggerProvider()));
        });
    }
}