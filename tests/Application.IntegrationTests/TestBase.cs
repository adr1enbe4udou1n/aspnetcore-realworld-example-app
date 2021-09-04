using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Xunit;

namespace Application.IntegrationTests
{
    [Collection("DB")]
    public class TestBase : IAsyncLifetime, IClassFixture<Startup>
    {
        protected readonly ServiceProvider _provider;
        protected readonly IConfiguration _configuration;

        protected readonly IMediator _mediator;

        protected readonly AppDbContext _context;

        protected readonly IPasswordHasher _passwordHasher;

        protected readonly IJwtTokenGenerator _jwtTokenGenerator;

        protected readonly ICurrentUser _currentUser;

        public TestBase(Startup factory)
        {
            _configuration = factory.Configuration;
            var provider = factory.Services.BuildServiceProvider();

            _mediator = provider.GetService<IMediator>();
            _passwordHasher = provider.GetService<IPasswordHasher>();
            _jwtTokenGenerator = provider.GetService<IJwtTokenGenerator>();
            _context = provider.GetService<AppDbContext>();
            _currentUser = provider.GetService<ICurrentUser>();
        }

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

        protected async Task<User> ActingAs(User user)
        {
            user.Password = _passwordHasher.Hash("password");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await _currentUser.SetIdentifier(user.Id);
            return user;
        }

        protected async Task<User> ActingAsExistingUser(string name)
        {
            var user = await _context.Users.Where(u => u.Name == name).SingleAsync();

            await _currentUser.SetIdentifier(user.Id);
            return user;
        }
    }
}