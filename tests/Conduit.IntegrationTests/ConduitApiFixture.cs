
using Conduit.Application.Interfaces;
using Conduit.Infrastructure.Persistence;
using Conduit.IntegrationTests.Events;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Testcontainers.PostgreSql;

using Xunit;

namespace Conduit.IntegrationTests;

public class ConduitApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("main")
        .WithUsername("main")
        .WithPassword("main")
        .WithImage("postgres:16")
        .WithNetworkAliases("postgres-test")
        .WithNetwork("rootless_default")
        .Build();

    public async Task MigrateDatabase()
    {
        using var scope = Services.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddLogging((builder) =>
                    builder.AddProvider(new SqlCounterLoggerProvider()));

                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<IAppDbContext, AppDbContext>((options) =>
                {
                    options
                        .UseLazyLoadingProxies()
                        .UseNpgsql(_postgreSqlContainer.GetConnectionString());
                });
            });
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await MigrateDatabase();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}