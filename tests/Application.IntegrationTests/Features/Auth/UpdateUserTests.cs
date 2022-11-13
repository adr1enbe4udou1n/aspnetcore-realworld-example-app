using System.Net;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Auth;

public class UpdateUserTests : TestBase
{
    public UpdateUserTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> InvalidInfos()
    {
        yield return new object[]
        {
            new UpdateUserDTO
            {
                Username = "John Doe",
                Email = "john.doe",
                Bio = "My Bio",
            },
        };
        yield return new object[]
        {
            new UpdateUserDTO
            {
                Username = "",
                Email = "john.doe@example.com",
                Bio = "My Bio",
            }
        };
    }

    [Theory, MemberData(nameof(InvalidInfos))]
    public async Task Cannot_Update_Infos_With_Invalid_Data(UpdateUserDTO user)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/user", new UpdateUserRequest(user));
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

        var request = new UpdateUserRequest(new UpdateUserDTO
        {
            Email = "jane.doe@example.com",
            Bio = "My Bio"
        });

        var currentUser = await Act<UserResponse>(HttpMethod.Put, "/user", request);

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("jane.doe@example.com");
        currentUser.User.Bio.Should().Be("My Bio");
        currentUser.User.Image.Should().BeNull();

        var created = await _context.Users.Where(u => u.Email == request.User.Email).SingleOrDefaultAsync();
        created.Should().NotBeNull();
    }

    [Fact]
    public async Task Guest_User_Cannot_Update_Infos()
    {
        var response = await Act(HttpMethod.Put, "/user", new UpdateUserRequest(
            new UpdateUserDTO
            {
                Email = "jane.doe@example.com"
            }
        ));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logged_User_Cannot_Update_With_Already_Used_Email()
    {
        await _context.Users.AddAsync(new User
        {
            Name = "John Doe",
            Email = "jane.doe@example.com"
        });
        await _context.SaveChangesAsync();

        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(
            HttpMethod.Put, "/user",
            new UpdateUserRequest(
                new UpdateUserDTO
                {
                    Email = "jane.doe@example.com",
                }
            ));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}