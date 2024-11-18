using Conduit.Infrastructure.Persistence;
using Conduit.Tools.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Npgsql;

namespace Conduit.Tools.Commands;

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

        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SET session_replication_role = 'replica';";
        await cmd.ExecuteNonQueryAsync();

        try
        {
            cmd.CommandText = @"
                TRUNCATE TABLE ""ArticleTag"" CASCADE;
                TRUNCATE TABLE ""ArticleFavorite"" CASCADE;
                TRUNCATE TABLE ""Comments"" CASCADE;
                TRUNCATE TABLE ""Articles"" CASCADE;
                TRUNCATE TABLE ""Tags"" CASCADE;
                TRUNCATE TABLE ""Users"" CASCADE;
            ";
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            cmd.CommandText = "SET session_replication_role = 'origin';";
            await cmd.ExecuteNonQueryAsync();
        }
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