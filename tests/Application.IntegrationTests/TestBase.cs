using Application.IntegrationTests.Events;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NUnit.Framework;
using Respawn;
using Respawn.Graph;

namespace Application.IntegrationTests;

public class TestBase
{
    protected IMediator _mediator;

    protected AppDbContext _context;

    protected IPasswordHasher _passwordHasher;

    protected IJwtTokenGenerator _jwtTokenGenerator;

    protected ICurrentUser _currentUser;

    private readonly string _connectionString;

    private IServiceCollection GetServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        return services
            .AddSingleton<IConfiguration>(configuration)
            .AddInfrastructure(configuration);
    }

    protected TestBase()
    {
        var _provider = GetServices().BuildServiceProvider();

        var configuration = _provider.GetRequiredService<IConfiguration>();
        _connectionString = configuration.GetConnectionString("DefaultConnection");

        var appDbContext = _provider.GetRequiredService<AppDbContext>();
        appDbContext.Database.Migrate();

        _mediator = _provider.GetRequiredService<IMediator>();
        _passwordHasher = _provider.GetRequiredService<IPasswordHasher>();
        _jwtTokenGenerator = _provider.GetRequiredService<IJwtTokenGenerator>();
        _context = _provider.GetRequiredService<AppDbContext>();
        _currentUser = _provider.GetRequiredService<ICurrentUser>();
    }

    [SetUp]
    public async Task RefreshDatabase()
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            var checkpoint = new Checkpoint
            {
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
                DbAdapter = DbAdapter.Postgres
            };
            await checkpoint.Reset(conn);
        }
    }

    private int _userId;

    protected async Task<User> ActingAs(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        await _currentUser.SetIdentifier(user.Id);
        _userId = user.Id;
        return user;
    }

    protected async Task<TResponse> Act<TResponse>(IRequest<TResponse> request)
    {
        SqlCounterLogger.ResetCounter();

        var provider = GetServices()
            .AddLogging((builder) => builder.AddProvider(new SqlCounterLoggerProvider()))
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
            Console.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter()}");
        }
    }
}