using Application.Exceptions;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Profiles;

public class ProfileGetTests : TestBase
{
    [Test]
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

        var response = await Act(new ProfileGetQuery("John Doe"));

        response.Profile.Should().BeEquivalentTo(new ProfileDTO
        {
            Username = "John Doe",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
            Following = false
        });
    }

    [Test]
    public async Task Cannot_Get_Non_Existent_Profile()
    {
        await this.Invoking(x => x.Act(new ProfileGetQuery("John Doe")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Can_Get_Followed_Profile()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Following = new List<FollowerUser>
                {
                    new FollowerUser
                    {
                        Following = new User
                        {
                            Name = "Jane Doe",
                            Email = "jane.doe@example.com",
                            Bio = "My Bio",
                            Image = "https://i.pravatar.cc/300",
                        }
                    }
                }
        });

        var response = await Act(new ProfileGetQuery("Jane Doe"));

        response.Profile.Should().BeEquivalentTo(new ProfileDTO
        {
            Username = "Jane Doe",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300",
            Following = true
        });
    }
}