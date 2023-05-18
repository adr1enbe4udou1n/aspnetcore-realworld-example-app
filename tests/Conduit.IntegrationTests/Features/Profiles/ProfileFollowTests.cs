using System.Net;

using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Profiles;

public class ProfileFollowTests : TestBase
{
    public ProfileFollowTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Guest_Cannot_Follow_Profile()
    {
        var response = await Act(HttpMethod.Post, "/profiles/john/follow");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Follow_Profile()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Context.Users.AddRangeAsync(
            new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            },
            new User
            {
                Name = "Alice",
                Email = "alice@example.com",
            }
        );
        await Context.SaveChangesAsync();

        var response = await Act<ProfileResponse>(HttpMethod.Post, "/profiles/Jane Doe/follow");

        response.Profile.Should().BeEquivalentTo(new
        {
            Username = "Jane Doe",
            Following = true
        });

        (await Context.Set<FollowerUser>().CountAsync()).Should().Be(1);
        (await Context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Jane Doe"))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Can_Unfollow_Profile()
    {
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        };

        user.AddFollowing(
            new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            },
            new User
            {
                Name = "Alice",
                Email = "alice@example.com",
            }
        );

        await ActingAs(user);

        var response = await Act<ProfileResponse>(HttpMethod.Delete, "/profiles/Jane Doe/follow");

        response.Profile.Should().BeEquivalentTo(new
        {
            Username = "Jane Doe",
            Following = false
        });

        (await Context.Set<FollowerUser>().CountAsync()).Should().Be(1);
        (await Context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Alice"))
            .Should().BeTrue();
    }
}