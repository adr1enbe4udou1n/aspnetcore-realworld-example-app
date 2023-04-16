using System.Net;

using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Auth;

public class CurrentUserTests : TestBase
{
    public CurrentUserTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
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

    [Fact]
    public async Task Guest_User_Cannot_Fetch_Infos()
    {
        var response = await Act(HttpMethod.Get, "/user");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}