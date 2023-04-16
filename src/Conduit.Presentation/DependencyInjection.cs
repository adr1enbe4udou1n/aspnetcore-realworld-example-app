using Conduit.Presentation.Converters;
using Conduit.Presentation.Extensions;
using Conduit.Presentation.Filters;
using Conduit.Presentation.OptionsSetup;

using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddRouting(options => options.LowercaseUrls = true)
            .AddControllers(opt =>
            {
                opt.UseRoutePrefix("api");
                opt.Filters.Add(typeof(ApiExceptionFilterAttribute));
            })
            .AddApplicationPart(typeof(DependencyInjection).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new DateTimeConverter())
            );

        return services
            .ConfigureOptions<SwaggerGenOptionsSetup>()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();
    }
}