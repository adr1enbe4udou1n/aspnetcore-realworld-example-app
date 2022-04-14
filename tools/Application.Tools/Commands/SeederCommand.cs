
using Application.Tools;
using Application.Tools.Interfaces;
using Cocona;

namespace Tools.Commands;

public class SeederCommand
{
    private readonly DatabaseManager _databaseManager;
    private readonly IEnumerable<ISeeder> _seeders;

    public SeederCommand(DatabaseManager databaseManager, IEnumerable<ISeeder> seeders)
    {
        _databaseManager = databaseManager;
        _seeders = seeders;
    }

    [Command("fresh", Description = "Wipe data")]
    public async Task FreshData()
    {
        await _databaseManager.Reset();
    }

    [Command("seed", Description = "Fake data")]
    public async Task SeedData()
    {
        await FreshData();

        var token = new CancellationToken();

        foreach (var seeder in _seeders)
        {
            await seeder.Run(token);
        }
    }
}