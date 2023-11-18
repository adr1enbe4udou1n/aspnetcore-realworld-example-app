using Conduit.Presentation.Converters;
using Conduit.Presentation.EndPoints;
using Conduit.Presentation.Filters;
using Conduit.Presentation.OptionsSetup;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Presentation;

public static class ServiceExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddRouting(options => options.LowercaseUrls = true)
            .Configure<JsonOptions>(options =>
                options.SerializerOptions.Converters.Add(new DateTimeConverter())
            );

        return services
            .ConfigureOptions<SwaggerGenOptionsSetup>()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();
    }

    public static void AddApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.AddArticlesEndpoints();
        api.AddCommentsEndpoints();
        api.AddProfilesEndpoints();
        api.AddTagsEndpoints();
        api.AddUserEndpoints();
        api.AddUsersEndpoints();

        api.AddEndpointFilter<ApiExceptionFilter>();
    }
}