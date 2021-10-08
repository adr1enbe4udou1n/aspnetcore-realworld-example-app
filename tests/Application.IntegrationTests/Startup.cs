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

        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var services = GetApplicationServices();
            var _provider = services.BuildServiceProvider();

            var appDbContext = _provider.GetService<AppDbContext>();

            appDbContext.Database.Migrate();
        }

        public IServiceCollection GetApplicationServices()
        {
            IServiceCollection services = new ServiceCollection();

            return services.AddInfrastructure(Configuration);
        }
    }
}
