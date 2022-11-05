using Application.Tools.Interfaces;
using Cocona;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Tools.Commands;

public class SeederCommand
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<ISeeder> _seeders;

    public SeederCommand(AppDbContext context, IEnumerable<ISeeder> seeders)
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

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        });

        await respawner.ResetAsync(conn);
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