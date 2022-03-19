using System.Globalization;
using Application.Features.Auth.Commands;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Auth;

public class RegisterTests : TestBase
{
    private static IEnumerable<TestCaseData> InvalidRegisters()
    {
        yield return new TestCaseData(new NewUserDTO
        {
            Email = "john.doe",
            Username = "John Doe",
            Password = "password",
        });
        yield return new TestCaseData(new NewUserDTO
        {
            Email = "john.doe@example.com",
        });
        yield return new TestCaseData(new NewUserDTO
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "pass",
        });
    }

    [Test, TestCaseSource(nameof(InvalidRegisters))]
    public async Task User_Cannot_Register_With_Invalid_Data(NewUserDTO user)
    {
        await this.Invoking(x => x.Act(new NewUserRequest(user)))
            .Should().ThrowAsync<ValidationException>()
            .Where(e => e.Errors.Any());
    }

    [Test]
    public async Task User_Can_Register()
    {
        var request = new NewUserRequest(new NewUserDTO
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "password",
        });

        var currentUser = await Act(request);

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("john.doe@example.com");

        var created = await Context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        created.Should().NotBeNull();

        PasswordHasher.Check("password", created!.Password!).Should().BeTrue();

        var payload = JwtTokenGenerator.DecodeToken(currentUser.User.Token);

        payload["id"].Should().Be(created.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }

    [Test]
    public async Task User_Cannot_Register_Twice()
    {
        await Context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = "password",
        });
        await Context.SaveChangesAsync();

        await this.Invoking(x => x.Act(
            new NewUserRequest(
                new NewUserDTO
                {
                    Email = "john.doe@example.com",
                    Username = "John Doe",
                    Password = "password",
                }
            )))
                .Should().ThrowAsync<ValidationException>()
                .Where(e => e.Errors.First(x => x.PropertyName == "User.Email")
                    .ErrorMessage == "Email is already used");
    }
}