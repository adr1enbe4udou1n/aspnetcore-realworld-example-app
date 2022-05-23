
using Application.Interfaces;
using Application.Tools.Interfaces;
using Cocona;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Tools.Commands;

public class SeederCommand
{
    private readonly IAppDbContext _context;
    private readonly IEnumerable<ISeeder> _seeders;

    public SeederCommand(IAppDbContext context, IEnumerable<ISeeder> seeders)
    {
        _context = context;
        _seeders = seeders;
    }

    [Command("migrate", Description = "Migrate database")]
    public async Task Migrate()
    {
        await _context.Database.MigrateAsync();
    }

    [Command("fresh", Description = "Wipe data")]
    public async Task FreshData()
    {
        await Migrate();

        using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());

        await conn.OpenAsync();

        var checkpoint = new Checkpoint
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        };
        await checkpoint.Reset(conn);
    }

    [Command("seed", Description = "Fake data")]
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
}