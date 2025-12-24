using Conduit.Presentation.Converters;
using Conduit.Presentation.Endpoints;
using Conduit.Presentation.Exceptions;
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
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<NotFoundExceptionHandler>()
            .AddExceptionHandler<ForbiddenExceptionHandler>()
            .AddProblemDetails()
            .ConfigureOptions<SwaggerGenOptionsSetup>()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.OperationFilter<ArticlesApiOperationFilter>();
                options.OperationFilter<CommentsApiOperationFilter>();
                options.OperationFilter<ProfilesApiOperationFilter>();
                options.OperationFilter<UserApiOperationFilter>();
                options.OperationFilter<UsersApiOperationFilter>();
            });
    }

    public static void AddApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api
            .AddUserRoutes()
            .AddUsersRoutes()
            .AddProfilesRoutes()
            .AddTagsRoutes()
            .AddArticlesRoutes()
            .AddCommentsRoutes();
    }
}