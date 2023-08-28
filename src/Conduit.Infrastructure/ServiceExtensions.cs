using Conduit.Application.Interfaces;
using Conduit.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Scrutor;

using Slugify;

namespace Conduit.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSource = new NpgsqlDataSourceBuilder(
                configuration.GetConnectionString("DefaultConnection")
            )
            .BuildMultiHost();

        return services
            .AddHttpContextAccessor()
            .AddDbContext<IAppDbContext, AppDbContext>((options) =>
            {
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(dataSource);
            })
            .Scan(
                selector => selector
                    .FromAssemblies(
                        typeof(ServiceExtensions).Assembly,
                        typeof(SlugHelper).Assembly
                    )
                    .AddClasses()
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
    }
}