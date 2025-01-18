using System.Net;

using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;


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

public class UpdateUserTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Theory, ClassData(typeof(InvalidInfos))]
    public async Task Cannot_Update_Infos_With_Invalid_Data(UpdateUserDto user)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/user", new UpdateUserCommand(user));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        Assert.Equal("John Doe", currentUser.User.Username);
        Assert.Equal("jane.doe@example.com", currentUser.User.Email);
        Assert.Equal("My Bio", currentUser.User.Bio);
        Assert.Null(currentUser.User.Image);

        var created = await Context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        Assert.NotNull(created);
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
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}