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
        private readonly ITestOutputHelper _output;
        private bool _logEnabled;

        private readonly IConfiguration _configuration;

        protected IMediator Mediator { get; private set; }

        protected AppDbContext Context { get; private set; }

        protected IPasswordHasher PasswordHasher { get; private set; }

        protected IJwtTokenGenerator JwtTokenGenerator { get; private set; }

        protected ICurrentUser CurrentUser { get; private set; }

        private readonly IServiceProvider _provider;

        public TestBase(Startup factory, ITestOutputHelper output)
        {
            _output = output;

            _configuration = factory.Configuration;
            _provider = factory.Services
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

            Mediator = _provider.GetRequiredService<IMediator>();
            PasswordHasher = _provider.GetRequiredService<IPasswordHasher>();
            JwtTokenGenerator = _provider.GetRequiredService<IJwtTokenGenerator>();
            Context = _provider.GetRequiredService<AppDbContext>();
            CurrentUser = _provider.GetRequiredService<ICurrentUser>();
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

        private int _userId;

        protected async Task<User> ActingAs(User user)
        {
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            await CurrentUser.SetIdentifier(user.Id);
            _userId = user.Id;
            return user;
        }

        protected async Task<TResponse> Act<TResponse>(IRequest<TResponse> request)
        {
            SqlCounterLogger.CurrentCounter = 0;

            using (var scope = _provider.CreateScope())
            {
                _logEnabled = true;

                var currentUser = scope.ServiceProvider.GetService<ICurrentUser>();
                await currentUser.SetIdentifier(_userId);

                var mediator = scope.ServiceProvider.GetService<IMediator>();
                var result = await mediator.Send<TResponse>(request);

                _logEnabled = false;

                _output.WriteLine($"SQL queries count : {SqlCounterLogger.CurrentCounter}");

                return result;
            }

        }
    }
}
