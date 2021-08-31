using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace Application.Tools
{
    public class DatabaseManager
    {
        private IConfiguration _configuration;

        public DatabaseManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Reset()
        {
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
}