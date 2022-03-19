using System.Globalization;
using Application.Features.Auth.Commands;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Auth;

public class LoginTests : TestBase
{
    private static IEnumerable<TestCaseData> InvalidCredentials()
    {
        yield return new TestCaseData(new LoginUserDTO
        {
            Email = "jane.doe@example.com",
            Password = "password",
        });
        yield return new TestCaseData(new LoginUserDTO
        {
            Email = "john.doe@example.com",
            Password = "badpassword",
        });
    }

    [Test, TestCaseSource(nameof(InvalidCredentials))]
    public async Task User_Cannot_Login_With_Invalid_Data(LoginUserDTO credentials)
    {
        await _context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = _passwordHasher.Hash("password"),
        });
        await _context.SaveChangesAsync();

        await this.Invoking(x => x.Act(new LoginUserRequest(credentials)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
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

        var payload = _jwtTokenGenerator.DecodeToken(currentUser.User.Token);

        payload["id"].Should().Be(user.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }
}