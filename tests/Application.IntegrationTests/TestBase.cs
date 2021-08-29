using System.Threading.Tasks;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using WebUI;
using Xunit;

namespace Application.IntegrationTests
{
    public class TestBase : IAsyncLifetime
    {
        private readonly IConfigurationRoot _configuration;

        private readonly ServiceProvider _provider;

        private readonly Checkpoint _checkpoint = new Checkpoint
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        };

        protected readonly IMediator _mediator;

        protected readonly AppDbContext _context;

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            using (var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();
                await _checkpoint.Reset(conn);
            }
        }

        public TestBase()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Testing.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var startup = new Startup(_configuration);
            var services = new ServiceCollection();

            startup.ConfigureServices(services);

            _provider = services.BuildServiceProvider();

            _mediator = _provider.GetService<IMediator>();
            _context = _provider.GetService<AppDbContext>();

            _context.Database.Migrate();
        }
    }
}
