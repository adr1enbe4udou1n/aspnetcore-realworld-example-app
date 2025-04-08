using System.Globalization;
using System.Net;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;
using Conduit.Presentation.Endpoints;

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

public class RegisterTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Theory, ClassData(typeof(InvalidRegisters))]
    public async Task User_Cannot_Register_With_Invalid_Data(NewUserDto user)
    {
        var response = await Act(HttpMethod.Post, "/users", new NewUserRequest(user));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User_Can_Register()
    {
        var request = new NewUserRequest(new NewUserDto
        {
            Email = "john.doe@example.com",
            Username = "John Doe",
            Password = "password",
        });

        var currentUser = await Act<UserResponse>(HttpMethod.Post, "/users", request);

        Assert.Equal("John Doe", currentUser.User.Username);
        Assert.Equal("john.doe@example.com", currentUser.User.Email);

        var created = await Context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        Assert.NotNull(created);

        Assert.True(PasswordHasher.Check("password", created!.Password!));

        var payload = DecodeToken(currentUser.User.Token);

        Assert.Equal(created.Id.ToString(CultureInfo.InvariantCulture), payload["sub"]);
        Assert.Equal("John Doe", payload["name"]);
        Assert.Equal("john.doe@example.com", payload["email"]);
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
            new NewUserRequest(
                new NewUserDto
                {
                    Email = "john.doe@example.com",
                    Username = "John Doe",
                    Password = "password",
                }
            ));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}