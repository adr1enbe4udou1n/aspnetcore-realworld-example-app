using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Auth;
using Domain.Entities;
using MediatR;
using Moq;
using Xunit;

namespace WebUI.IntegrationTests
{
    public class UsersTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly Mock<IMediator> _mediatorMock;

        public UsersTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            _mediatorMock = factory.MediatorMock;
        }

        [Fact]
        public async Task RegisterTest()
        {
            var httpResponse = await _client.PostAsJsonAsync("/users", new RegisterCommand(
                new RegisterDTO
                {
                    Email = "john.doe@example.com",
                    Username = "John Doe",
                    Password = "password",
                }
            ));

            httpResponse.EnsureSuccessStatusCode();

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()), Times.Once()
            );
        }

        [Fact]
        public async Task LoginTest()
        {
            var httpResponse = await _client.PostAsJsonAsync("/users/login", new LoginCommand(
                new LoginDTO
                {
                    Email = "john.doe@example.com",
                    Password = "password",
                }
            ));

            httpResponse.EnsureSuccessStatusCode();

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Once()
            );
        }
    }
}
