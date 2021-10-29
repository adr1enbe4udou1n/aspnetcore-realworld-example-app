
using Application;
using Application.Infrastructure.Security;
using Application.Infrastructure.Settings;
using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slugify;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddApplication()
                .AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>())
                .AddDbContext<AppDbContext>(options =>
                {
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                })
                .AddScoped<ICurrentUser, CurrentUser>()
                .AddScoped<IPasswordHasher, PasswordHasher>()
                .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
                    .Configure<JwtOptions>(configuration.GetSection("Jwt"))
                .AddScoped<ISlugHelper, SlugHelper>()
                .AddScoped<ISlugifier, Slugifier>();
        }
    }
}
