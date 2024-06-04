using Conduit.Infrastructure.Persistence;
using Conduit.Tools.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;

using Respawn;

namespace Tools.Commands;

public class SeederCommand(IConfiguration config, AppDbContext context, IEnumerable<ISeeder> seeders) : IAsyncDisposable
{
    /// <summary>
    /// Migrate database
    /// </summary>
    public async Task Migrate()
    {
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Wipe data
    /// </summary>
    public async Task Fresh()
    {
        await Migrate();

        using var conn = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));

        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            DbAdapter = DbAdapter.Postgres
        });

        await respawner.ResetAsync(conn);
    }

    /// <summary>
    /// Fake data
    /// </summary>
    public async Task Seed()
    {
        await Migrate();
        await Fresh();

        var token = new CancellationToken();

        foreach (var seeder in seeders)
        {
            await seeder.Run(token);
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return context.DisposeAsync();
    }
}