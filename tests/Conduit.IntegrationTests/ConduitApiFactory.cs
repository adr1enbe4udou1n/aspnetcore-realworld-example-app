using Conduit.Infrastructure.Persistence;
using Conduit.IntegrationTests.Events;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Respawn;
using Respawn.Graph;

namespace Conduit.IntegrationTests;

public class ConduitApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ConduitApiFactory()
    {
        using var scope = Services.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _connectionString = dbContext.Database.GetDbConnection().ConnectionString;

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

    public async Task RefreshDatabase()
    {
        using var conn = new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        });

        await respawner.ResetAsync(conn);
    }
}