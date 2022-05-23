using System.Net;
using Application.Features.Auth.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Auth;

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

        var response = await Act<UserResponse>(HttpMethod.Get, "/user");

        response.User.Username.Should().Be("John Doe");
        response.User.Email.Should().Be("john.doe@example.com");
    }

    [Test]
    public async Task Guest_User_Cannot_Fetch_Infos()
    {
        var response = await Act(HttpMethod.Get, "/user");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}