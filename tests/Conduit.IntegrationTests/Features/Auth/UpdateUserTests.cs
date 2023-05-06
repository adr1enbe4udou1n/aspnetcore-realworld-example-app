using System.Net;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Auth;

public class InvalidInfos : TheoryData<UpdateUserDto>
{
    public InvalidInfos()
    {
        Add(new UpdateUserDto
        {
            Username = "John Doe",
            Email = "john.doe",
            Bio = "My Bio",
        });
        Add(new UpdateUserDto
        {
            Username = "",
            Email = "john.doe@example.com",
            Bio = "My Bio",
        });
    }
}

public class UpdateUserTests : TestBase
{
    public UpdateUserTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Theory, ClassData(typeof(InvalidInfos))]
    public async Task Cannot_Update_Infos_With_Invalid_Data(UpdateUserDto user)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/user", new UpdateUserCommand(user));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logged_User_Can_Update_Infos()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var request = new UpdateUserCommand(new UpdateUserDto
        {
            Email = "jane.doe@example.com",
            Bio = "My Bio"
        });

        var currentUser = await Act<UserResponse>(HttpMethod.Put, "/user", request);

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("jane.doe@example.com");
        currentUser.User.Bio.Should().Be("My Bio");
        currentUser.User.Image.Should().BeNull();

        var created = await Context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        created.Should().NotBeNull();
    }

    [Fact]
    public async Task Guest_User_Cannot_Update_Infos()
    {
        var response = await Act(HttpMethod.Put, "/user", new UpdateUserCommand(
            new UpdateUserDto
            {
                Email = "jane.doe@example.com"
            }
        ));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logged_User_Cannot_Update_With_Already_Used_Email()
    {
        await Context.Users.AddAsync(new User
        {
            Name = "John Doe",
            Email = "jane.doe@example.com"
        });
        await Context.SaveChangesAsync();

        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(
            HttpMethod.Put, "/user",
            new UpdateUserCommand(
                new UpdateUserDto
                {
                    Email = "jane.doe@example.com",
                }
            ));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}