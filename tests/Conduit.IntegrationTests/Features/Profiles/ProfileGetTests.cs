using System.Net;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Profiles;

public class ProfileGetTests : TestBase
{
    public ProfileGetTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

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

        var response = await Act<ProfileResponse>(HttpMethod.Get, "/profiles/celeb_John Doe");

        response.Profile.Should().BeEquivalentTo(new ProfileDto
        {
            Username = "John Doe",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
            Following = false
        });
    }

    [Fact]
    public async Task Cannot_Get_Non_Existent_Profile()
    {
        var response = await Act(HttpMethod.Get, "/profiles/celeb_John Doe");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

        var response = await Act<ProfileResponse>(HttpMethod.Get, "/profiles/celeb_Jane Doe");

        response.Profile.Should().BeEquivalentTo(new ProfileDto
        {
            Username = "Jane Doe",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
            Following = true
        });
    }
}