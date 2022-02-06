using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Respawn.Graph;

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

        var checkpoint = new Checkpoint
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
        };
        await checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));
    }
}