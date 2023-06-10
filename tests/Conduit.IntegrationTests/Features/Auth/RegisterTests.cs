using System.Globalization;
using System.Net;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Auth;

public class InvalidRegisters : TheoryData<NewUserDto>
{
    public InvalidRegisters()
    {
        Add(new NewUserDto
        {
            Email = "john.doe",
            Username = "John Doe",
            Password = "password",
        });
        Add(new NewUserDto
        {
            Email = "john.doe@example.com",
            Username = "",
            Password = "",
        });
        Add(new NewUserDto
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "pass",
        });
    }
}

public class RegisterTests : TestBase
{
    public RegisterTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Theory, ClassData(typeof(InvalidRegisters))]
    public async Task User_Cannot_Register_With_Invalid_Data(NewUserDto user)
    {
        var response = await Act(HttpMethod.Post, "/users", new NewUserCommand(user));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task User_Can_Register()
    {
        var request = new NewUserCommand(new NewUserDto
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "password",
        });

        var currentUser = await Act<UserResponse>(HttpMethod.Post, "/users", request);

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("john.doe@example.com");

        var created = await Context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        created.Should().NotBeNull();

        PasswordHasher.Check("password", created!.Password!).Should().BeTrue();

        var payload = DecodeToken(currentUser.User.Token);

        payload["sub"].Should().Be(created.Id.ToString(CultureInfo.InvariantCulture));
        payload["name"].Should().Be("John Doe");
        payload["email"].Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task User_Cannot_Register_Twice()
    {
        await Context.Users.AddAsync(new User
        {
            Email = "john.doe@example.com",
            Name = "John Doe",
            Password = "password",
        });
        await Context.SaveChangesAsync();

        var response = await Act(
            HttpMethod.Post, "/users",
            new NewUserCommand(
                new NewUserDto
                {
                    Email = "john.doe@example.com",
                    Username = "John Doe",
                    Password = "password",
                }
            ));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}