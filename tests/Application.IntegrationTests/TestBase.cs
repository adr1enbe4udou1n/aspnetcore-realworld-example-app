using System.Net.Http.Json;
using Application.IntegrationTests.Events;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    protected TestBase()
    {
        _connectionString = "Server=localhost;Port=5434;User Id=main;Password=main;Database=main;";

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _connectionString
        );
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "super secret key");

        var application = new ConduitApiApplicationFactory();

        var scope = application.Services.CreateScope();

        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
        _passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        _jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        _context.Database.Migrate();
    }

    [SetUp]
    public async Task RefreshDatabase()
    {
        using var conn = new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        var checkpoint = new Checkpoint
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        };
        await checkpoint.Reset(conn);
    }

    private string? _token = null;

    protected async Task<User> ActingAs(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _token = _jwtTokenGenerator.CreateToken(user);
        await _currentUser.SetIdentifier(user.Id);
        return user;
    }

    private HttpClient GetClient()
    {
        SqlCounterLogger.ResetCounter();

        var application = new ConduitApiApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddLogging((builder) => builder.AddProvider(new SqlCounterLoggerProvider()));
            });
        });

        var client = application.CreateClient();

        if (_token != null)
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Token {_token}");
        }

        return client;
    }


    protected async Task<HttpResponseMessage> Act(HttpMethod method, string requestUri)
    {
        var client = GetClient();

        try
        {
            return await client.SendAsync(new HttpRequestMessage(method, $"/api{requestUri}"));
        }
        finally
        {
            Console.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter()}");
        }
    }

    protected async Task<HttpResponseMessage> Act(HttpMethod method, string requestUri, object value)
    {
        var client = GetClient();

        try
        {
            if (value != null)
            {
                switch (method.Method)
                {
                    case "POST":
                        return await client.PostAsJsonAsync($"/api{requestUri}", value);
                    case "PUT":
                        return await client.PutAsJsonAsync($"/api{requestUri}", value);
                }
            }
            return await client.SendAsync(new HttpRequestMessage(method, $"/api{requestUri}"));
        }
        finally
        {
            Console.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter()}");
        }
    }

    protected async Task<T> Act<T>(HttpMethod method, string requestUri)
    {
        var response = await Act(method, requestUri);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    protected async Task<T> Act<T>(HttpMethod method, string requestUri, object value)
    {
        var response = await Act(method, requestUri, value);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
}