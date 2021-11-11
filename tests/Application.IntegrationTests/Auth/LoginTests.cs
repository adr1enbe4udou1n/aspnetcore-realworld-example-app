using System.Globalization;
using Application.Features.Auth.Commands;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Auth;

public class LoginTests : TestBase
{
    public LoginTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new LoginUserDTO {
                Email = "jane.doe@example.com",
                Password = "password",
            } },
            new [] { new LoginUserDTO {
                Email = "john.doe@example.com",
                Password = "badpassword",
            } },
        };

    [Theory]
    [MemberData(nameof(Data))]
    public async Task User_Cannot_Login_With_Invalid_Data(LoginUserDTO credentials)
    {
        await Context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = PasswordHasher.Hash("password"),
        });
        await Context.SaveChangesAsync();

        await this.Invoking(x => x.Act(new LoginUserRequest(credentials)))
            .Should().ThrowAsync<ValidationException>();
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

        var currentUser = await Act(
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

        var payload = JwtTokenGenerator.DecodeToken(currentUser.User.Token);

        payload["id"].Should().Be(user.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }
}
