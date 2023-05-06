using System.Globalization;
using System.Net;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Auth;

public class InvalidCredentials : TheoryData<LoginUserDto>
{
    public InvalidCredentials()
    {
        Add(new LoginUserDto
        {
            Email = "jane.doe@example.com",
            Password = "password",
        });
        Add(new LoginUserDto
        {
            Email = "john.doe@example.com",
            Password = "badpassword"
        });
    }
}

public class LoginTests : TestBase
{
    public LoginTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Theory, ClassData(typeof(InvalidCredentials))]
    public async Task User_Cannot_Login_With_Invalid_Data(LoginUserDto credentials)
    {
        await Context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = PasswordHasher.Hash("password"),
        });
        await Context.SaveChangesAsync();

        var response = await Act(HttpMethod.Post, "/users/login", new LoginUserCommand(credentials));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task User_Can_Login()
    {
        var user = new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = PasswordHasher.Hash("password"),
        };
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var currentUser = await Act<UserResponse>(
            HttpMethod.Post, "/users/login",
            new LoginUserCommand(
                new LoginUserDto
                {
                    Email = "john.doe@example.com",
                    Password = "password",
                }
            )
        );

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("john.doe@example.com");

        var payload = DecodeToken(currentUser.User.Token);

        payload["sub"].Should().Be(user.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }
}