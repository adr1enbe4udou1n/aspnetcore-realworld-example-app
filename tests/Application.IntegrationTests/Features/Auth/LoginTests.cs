using System.Globalization;
using System.Net;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Auth;

public class LoginTests : TestBase
{
    public LoginTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> InvalidCredentials()
    {
        yield return new object[]
        {
            new LoginUserDTO
            {
                Email = "jane.doe@example.com",
                Password = "password",
            },
        };
        yield return new object[]
        {
            new LoginUserDTO
            {
                Email = "john.doe@example.com",
                Password = "badpassword",
            }
        };
    }

    [Theory, MemberData(nameof(InvalidCredentials))]
    public async Task User_Cannot_Login_With_Invalid_Data(LoginUserDTO credentials)
    {
        await _context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = _passwordHasher.Hash("password"),
        });
        await _context.SaveChangesAsync();

        var response = await Act(HttpMethod.Post, "/users/login", new LoginUserRequest(credentials));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task User_Can_Login()
    {
        var user = new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = _passwordHasher.Hash("password"),
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var currentUser = await Act<UserResponse>(
            HttpMethod.Post, "/users/login",
            new LoginUserRequest(
                new LoginUserDTO
                {
                    Email = "john.doe@example.com",
                    Password = "password",
                }
            )
        );

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("john.doe@example.com");

        var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

        payload["id"].Should().Be(user.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }
}