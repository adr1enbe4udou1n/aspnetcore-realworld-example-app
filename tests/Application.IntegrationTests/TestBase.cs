using System;
using System.Linq;
using System.Threading.Tasks;
using Application.IntegrationTests.Events;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Xunit;
using Xunit.Abstractions;

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

        protected bool _logEnabled = false;

        protected readonly ITestOutputHelper _output;

        public TestBase(Startup factory, ITestOutputHelper output)
        {
            _output = output;

            _configuration = factory.Configuration;
            var provider = factory.Services
                .AddLogging((builder) => builder
                    .AddProvider(new SqlCounterLoggerProvider(() => _logEnabled))
                    .AddXUnit(output, options =>
                    {
                        options.Filter = (category, level) =>
                        {
                            return category == DbLoggerCategory.Database.Command.Name && _logEnabled;
                        };
                    }))
                .BuildServiceProvider();

            _mediator = provider.GetRequiredService<IMediator>();
            _passwordHasher = provider.GetRequiredService<IPasswordHasher>();
            _jwtTokenGenerator = provider.GetRequiredService<IJwtTokenGenerator>();
            _context = provider.GetRequiredService<AppDbContext>();
            _currentUser = provider.GetRequiredService<ICurrentUser>();
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

        protected async Task<TResult> Act<TResult>(Func<Task<TResult>> action)
        {
            _context.ChangeTracker.Clear();
            SqlCounterLogger.CurrentCounter = 0;

            _logEnabled = true;
            await _currentUser.Fresh();
            var result = await action();
            _logEnabled = false;

            _output.WriteLine($"SQL queries count : {SqlCounterLogger.CurrentCounter}");

            return result;
        }
    }
}