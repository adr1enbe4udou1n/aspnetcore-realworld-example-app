using System.Threading.Tasks;
using Application.Interfaces;
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
    [Collection("DB")]
    public class TestBase : IAsyncLifetime
    {
        private readonly IConfigurationRoot _configuration;

        private readonly ServiceProvider _provider;

        protected readonly IMediator _mediator;

        protected readonly AppDbContext _context;

        protected readonly IPasswordHasher _passwordHasher;

        protected readonly IJwtTokenGenerator _jwtTokenGenerator;

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            using (var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
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
            _passwordHasher = _provider.GetService<IPasswordHasher>();
            _jwtTokenGenerator = _provider.GetService<IJwtTokenGenerator>();
            _context = _provider.GetService<AppDbContext>();

            _context.Database.Migrate();
        }
    }
}
