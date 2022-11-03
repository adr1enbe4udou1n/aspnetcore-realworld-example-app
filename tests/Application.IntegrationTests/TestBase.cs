using System.Net.Http.Json;
using Application.IntegrationTests.Events;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

[Collection("Test collection")]
public class TestBase : IAsyncLifetime
{
    protected IMediator _mediator;

    protected IAppDbContext _context;

    protected IPasswordHasher _passwordHasher;

    protected IJwtTokenGenerator _jwtTokenGenerator;

    protected ICurrentUser _currentUser;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    protected TestBase(ConduitApiFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;

        var scope = factory.Services.CreateScope();

        _context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
        _passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        _jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
    }

    private async Task RefreshDatabase()
    {
        using var conn = new NpgsqlConnection(_context.Database.GetDbConnection().ConnectionString);

        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        });

        await respawner.ResetAsync(conn);
    }

    public async Task InitializeAsync() => await RefreshDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

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

        if (_token != null)
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Token {_token}");
        }

        return _client;
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
            _output.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter()}");
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
            _output.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter()}");
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