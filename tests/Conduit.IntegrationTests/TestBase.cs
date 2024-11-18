using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Infrastructure.Persistence;
using Conduit.IntegrationTests.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests;

[CollectionDefinition(nameof(DatabaseCollectionTest))]
public class DatabaseCollectionTest : ICollectionFixture<ConduitApiFixture>;

[Collection(nameof(DatabaseCollectionTest))]
public class TestBase(ConduitApiFixture factory, ITestOutputHelper output) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly IServiceScope _scope = factory.Services.CreateScope();
    protected AppDbContext Context => _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    protected IMediator Mediator => _scope.ServiceProvider.GetRequiredService<IMediator>();
    protected IPasswordHasher PasswordHasher => _scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    private IJwtTokenGenerator JwtTokenGenerator => _scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
    private ICurrentUser CurrentUser => _scope.ServiceProvider.GetRequiredService<ICurrentUser>();

    public async Task RefreshDatabase()
    {
        using var conn = new NpgsqlConnection(Context.Database.GetConnectionString());

        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();

        cmd.CommandText = "SET session_replication_role = 'replica';";
        await cmd.ExecuteNonQueryAsync();

        try
        {
            cmd.CommandText = @"
                TRUNCATE TABLE ""ArticleTag"" CASCADE;
                TRUNCATE TABLE ""ArticleFavorite"" CASCADE;
                TRUNCATE TABLE ""Comments"" CASCADE;
                TRUNCATE TABLE ""Articles"" CASCADE;
                TRUNCATE TABLE ""Tags"" CASCADE;
                TRUNCATE TABLE ""Users"" CASCADE;
            ";
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            cmd.CommandText = "SET session_replication_role = 'origin';";
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await RefreshDatabase();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }

    private string? _token;

    protected async Task<User> ActingAs(User user)
    {
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        _token = JwtTokenGenerator.CreateToken(user);
        await CurrentUser.SetIdentifier(user.Id);
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

    protected async Task<HttpResponseMessage> Act(HttpMethod method, string requestPath)
    {
        var client = GetClient();

        using var request = new HttpRequestMessage(method, $"/api{requestPath}");

        try
        {
            return await client.SendAsync(request);
        }
        finally
        {
            output.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter}");
        }
    }

    protected async Task<HttpResponseMessage> Act(HttpMethod method, string requestPath, object value)
    {
        var client = GetClient();

        using var request = new HttpRequestMessage(method, $"/api{requestPath}");

        try
        {
            if (value != null)
            {
                switch (method.Method)
                {
                    case "POST":
                        return await client.PostAsJsonAsync($"/api{requestPath}", value);
                    case "PUT":
                        return await client.PutAsJsonAsync($"/api{requestPath}", value);
                    default:
                        break;
                }
            }
            return await client.SendAsync(request);
        }
        finally
        {
            output.WriteLine($"SQL queries count : {SqlCounterLogger.GetCounter}");
        }
    }

    protected async Task<T> Act<T>(HttpMethod method, string requestPath)
    {
        var response = await Act(method, requestPath);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    protected async Task<T> Act<T>(HttpMethod method, string requestPath, object value)
    {
        var response = await Act(method, requestPath, value);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    protected static IDictionary<string, string> DecodeToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
#pragma warning disable CA5404
            ValidateIssuer = false,
            ValidateAudience = false,
#pragma warning restore CA5404
            ValidateIssuerSigningKey = true,
#pragma warning disable S6781 // JWT secret keys should not be disclosed
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("00000000-0000-0000-0000-000000000000")
            ),
#pragma warning restore S6781 // JWT secret keys should not be disclosed
        }, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        return jwtToken.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(group => group.Key, group => group.Last().Value);
    }
}