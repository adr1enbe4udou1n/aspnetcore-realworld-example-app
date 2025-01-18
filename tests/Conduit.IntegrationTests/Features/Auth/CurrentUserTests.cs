using System.Net;

using Conduit.Application.Features.Auth.Queries;
using Conduit.Domain.Entities;


using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Auth;

public class CurrentUserTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Logged_User_Can_Fetch_Infos()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act<UserResponse>(HttpMethod.Get, "/user");

        Assert.Equal("John Doe", response.User.Username);
        Assert.Equal("john.doe@example.com", response.User.Email);
    }

    [Fact]
    public async Task Guest_User_Cannot_Fetch_Infos()
    {
        var response = await Act(HttpMethod.Get, "/user");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}