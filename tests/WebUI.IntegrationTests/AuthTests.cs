using System.Net.Http.Json;
using Application.Features.Auth.Commands;
using MediatR;
using Moq;
using Xunit;

namespace WebUI.IntegrationTests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory<Startup>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMediator> _mediatorMock;

    public AuthTests(CustomWebApplicationFactory<Startup> factory)
    {
        _client = factory.CreateClient();
        _mediatorMock = factory.MediatorMock;
    }

    [Fact]
    public async Task Can_Use_Users_Route()
    {
        var httpResponse = await _client.PostAsJsonAsync("/api/users", new NewUserRequest(
            new NewUserDTO()
        ));

        httpResponse.EnsureSuccessStatusCode();

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<NewUserRequest>(), It.IsAny<CancellationToken>()), Times.Once()
        );
    }

    [Fact]
    public async Task Can_Use_Login_Route()
    {
        var httpResponse = await _client.PostAsJsonAsync("/api/users/login", new LoginUserRequest(
            new LoginUserDTO()
        ));

        httpResponse.EnsureSuccessStatusCode();

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<LoginUserRequest>(), It.IsAny<CancellationToken>()), Times.Once()
        );
    }
}