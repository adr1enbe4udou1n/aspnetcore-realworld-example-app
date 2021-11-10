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

namespace Application.IntegrationTests;

[Collection("DB")]
public class TestBase : IAsyncLifetime, IClassFixture<Startup>
{
    private readonly ITestOutputHelper _output;

    protected IMediator Mediator { get; private set; }

    protected AppDbContext Context { get; private set; }

    protected IPasswordHasher PasswordHasher { get; private set; }

    protected IJwtTokenGenerator JwtTokenGenerator { get; private set; }

    protected ICurrentUser CurrentUser { get; private set; }

    private readonly Startup _factory;

    public TestBase(Startup factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;

        var provider = factory.GetApplicationServices().BuildServiceProvider();

        Mediator = provider.GetRequiredService<IMediator>();
        PasswordHasher = provider.GetRequiredService<IPasswordHasher>();
        JwtTokenGenerator = provider.GetRequiredService<IJwtTokenGenerator>();
        Context = provider.GetRequiredService<AppDbContext>();
        CurrentUser = provider.GetRequiredService<ICurrentUser>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        using (var conn = new NpgsqlConnection(_factory.Configuration.GetConnectionString("DefaultConnection")))
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
        SqlCounterLogger.ResetCounter();

        var provider = _factory.GetApplicationServices()
            .AddLogging((builder) => builder
                .AddProvider(new SqlCounterLoggerProvider())
                .AddXUnit(_output, options =>
                {
                    options.Filter = (category, level) =>
                    {
                        return category == DbLoggerCategory.Database.Command.Name;
                    };
                }))
            .BuildServiceProvider();

        using var scope = provider.CreateScope();

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
        await currentUser.SetIdentifier(_userId);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            return await mediator.Send<TResponse>(request);
        }
        finally
        {
            _output.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter()}");
        }
    }
}
