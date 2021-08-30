using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace WebUI.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
    {
        public Mock<IMediator> MediatorMock { get; private set; } = new Mock<IMediator>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();

                services.AddScoped<IMediator>(options => MediatorMock.Object);
            });
        }
    }
}