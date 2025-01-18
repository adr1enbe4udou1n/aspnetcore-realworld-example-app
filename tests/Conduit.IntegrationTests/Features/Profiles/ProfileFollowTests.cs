using System.Net;

using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;


using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Profiles;

public class ProfileFollowTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Guest_Cannot_Follow_Profile()
    {
        var response = await Act(HttpMethod.Post, "/profiles/john/follow");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

        Assert.Equivalent(new
        {
            Username = "Jane Doe",
            Following = true
        }, response.Profile);

        Assert.Equal(1, await Context.Set<FollowerUser>().CountAsync());
        Assert.True(await Context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Jane Doe"));
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

        Assert.Equivalent(new
        {
            Username = "Jane Doe",
            Following = false
        }, response.Profile);

        Assert.Equal(1, await Context.Set<FollowerUser>().CountAsync());
        Assert.True(await Context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Alice"));
    }
}