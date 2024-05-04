using Conduit.Infrastructure.Persistence;
using Conduit.Tools.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;

using Respawn;
using Respawn.Graph;

namespace Tools.Commands;


[Command("db")]
public class SeederCommand(IConfiguration config, AppDbContext context, IEnumerable<ISeeder> seeders) : ConsoleAppBase, IAsyncDisposable
{
    [Command("migrate", "Migrate database")]
    public async Task Migrate()
    {
        await context.Database.MigrateAsync();
    }

    [Command("fresh", "Wipe data")]
    public async Task FreshData()
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

    [Command("seed", "Fake data")]
    public async Task SeedData()
    {
        await Migrate();
        await FreshData();

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