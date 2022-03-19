using Application.Exceptions;
using Application.Features.Auth.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Auth;

public class CurrentUserTests : TestBase
{
    [Test]
    public async Task Logged_User_Can_Fetch_Infos()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var currentUser = await Act(new CurrentUserQuery());

        currentUser.User.Username.Should().Be("John Doe");
        currentUser.User.Email.Should().Be("john.doe@example.com");
    }

    [Test]
    public async Task Guest_User_Cannot_Fetch_Infos()
    {
        await this.Invoking(x => x.Act(new CurrentUserQuery()))
            .Should().ThrowAsync<UnauthorizedException>();
    }
}