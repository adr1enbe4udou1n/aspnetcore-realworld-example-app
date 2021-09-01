using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceCollection Services { get; private set; } = new ServiceCollection();

        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            Services.AddInfrastructure(Configuration);

            var _provider = Services.BuildServiceProvider();

            var appDbContext = _provider.GetService<AppDbContext>();

            appDbContext.Database.Migrate();
        }
    }
}
