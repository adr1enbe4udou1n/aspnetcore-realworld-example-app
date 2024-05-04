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
    private readonly IConfiguration _config = config;
    private readonly AppDbContext _context = context;
    private readonly IEnumerable<ISeeder> _seeders = seeders;

    [Command("migrate", "Migrate database")]
    public async Task Migrate()
    {
        await _context.Database.MigrateAsync();
    }

    [Command("fresh", "Wipe data")]
    public async Task FreshData()
    {
        await Migrate();

        using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
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

        foreach (var seeder in _seeders)
        {
            await seeder.Run(token);
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _context.DisposeAsync();
    }
}