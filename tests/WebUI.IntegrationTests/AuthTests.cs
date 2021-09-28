using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using MediatR;
using Moq;
using Xunit;

namespace WebUI.IntegrationTests
{
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
        public async Task CanUseUsersRoute()
        {
            var httpResponse = await _client.PostAsJsonAsync("/api/users", new RegisterCommand(
                new RegisterDTO()
            ));

            httpResponse.EnsureSuccessStatusCode();

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()), Times.Once()
            );
        }

        [Fact]
        public async Task CanUseLoginRoute()
        {
            var httpResponse = await _client.PostAsJsonAsync("/api/users/login", new LoginCommand(
                new LoginDTO()
            ));

            httpResponse.EnsureSuccessStatusCode();

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Once()
            );
        }
    }
}
