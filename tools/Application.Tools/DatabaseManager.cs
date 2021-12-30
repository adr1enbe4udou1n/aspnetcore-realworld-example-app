using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace Application.Tools;

public class DatabaseManager
{
    private readonly IConfiguration _configuration;
    private readonly IAppDbContext _context;

    public DatabaseManager(IConfiguration configuration, IAppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task Reset()
    {
        await _context.Database.MigrateAsync();

        using (var conn = new NpgsqlConnection(
            _configuration.GetConnectionString("DefaultConnection")
        ))
        {
            await conn.OpenAsync();

            var checkpoint = new Checkpoint
            {
                TablesToIgnore = new[] { "__EFMigrationsHistory" },
                DbAdapter = DbAdapter.Postgres
            };
            await checkpoint.Reset(conn);
        }
    }
}