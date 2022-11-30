
using Application;
using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slugify;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddApplication()
            .AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>())
            .AddDbContextFactory<AppDbContext>((options) =>
            {
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            })
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped<IPasswordHasher, PasswordHasher>()
            .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
            .AddScoped<ISlugHelper, SlugHelper>()
            .AddScoped<ISlugifier, Slugifier>();
    }
}