using System.Net;

using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;


using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Profiles;

public class ProfileGetTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Can_Get_Profile()
    {
        await Context.Users.AddAsync(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
        });
        await Context.SaveChangesAsync();

        var response = await Act<ProfileResponse>(HttpMethod.Get, "/profiles/John Doe");

        Assert.Equivalent(new
        {
            Username = "John Doe",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
            Following = false
        }, response.Profile);
    }

    [Fact]
    public async Task Cannot_Get_Non_Existent_Profile()
    {
        var response = await Act(HttpMethod.Get, "/profiles/John Doe");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Can_Get_Followed_Profile()
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
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300",
            }
        );

        await ActingAs(user);

        var response = await Act<ProfileResponse>(HttpMethod.Get, "/profiles/Jane Doe");

        Assert.Equivalent(new
        {
            Username = "Jane Doe",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
            Following = true
        }, response.Profile);
    }
}