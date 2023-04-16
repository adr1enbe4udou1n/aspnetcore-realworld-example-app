using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Infrastructure.Persistence;
using Conduit.IntegrationTests.Events;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests;

[Collection("Test collection")]
public class TestBase : IAsyncLifetime
{
    protected AppDbContext Context { get; }
    protected IMediator Mediator { get; }
    protected IPasswordHasher PasswordHasher { get; }

    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly HttpClient _client;
    private readonly Func<Task> _refreshDatabase;
    private readonly ITestOutputHelper _output;

    protected TestBase(ConduitApiFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _refreshDatabase = factory.RefreshDatabase;
        _output = output;

        var scope = factory.Services.CreateScope();

        Context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
        PasswordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        _jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
    }

    public Task InitializeAsync()
    {
        return _refreshDatabase();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private string? _token;

    protected async Task<User> ActingAs(User user)
    {
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

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
                    default:
                        break;
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

    protected static IDictionary<string, string> DecodeToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("super-secret-key-value!")
            ),
        }, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        return jwtToken.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(group => group.Key, group => group.Last().Value);
    }
}