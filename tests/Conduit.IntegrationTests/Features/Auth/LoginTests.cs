using System.Globalization;
using System.Net;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;


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

public class LoginTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        Assert.Equal("John Doe", currentUser.User.Username);
        Assert.Equal("john.doe@example.com", currentUser.User.Email);

        var payload = DecodeToken(currentUser.User.Token);

        Assert.Equal(user.Id.ToString(CultureInfo.InvariantCulture), payload["sub"]);
        Assert.Equal("John Doe", payload["name"]);
        Assert.Equal("john.doe@example.com", payload["email"]);
    }
}