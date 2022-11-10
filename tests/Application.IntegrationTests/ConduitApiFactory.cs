using Application.IntegrationTests.Events;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Application.IntegrationTests;

public class ConduitApiFactory : WebApplicationFactory<Program>
{
    private readonly AppDbContext _context;
    private readonly string _connectionString;

    public ConduitApiFactory()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5434;User Id=main;Password=main;Database=main;";

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _connectionString);
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "super secret key");

        var scope = Services.CreateScope();

        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _context.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLogging((builder) => builder.AddProvider(new SqlCounterLoggerProvider()));
        });
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