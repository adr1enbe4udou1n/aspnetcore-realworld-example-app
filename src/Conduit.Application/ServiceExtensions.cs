using System.Reflection;

using Conduit.Application.Behaviors;
using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Commands;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Features.Tags.Queries;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(CommandBehavior<,>))
            .AddScoped<IQueryTags, QueryTags>()
            .AddScoped<IQueryProfiles, QueryProfiles>()
            .AddScoped<ICommandProfiles, CommandProfiles>()
            .AddScoped<IQueryUsers, QueryUsers>()
            .AddScoped<ICommandUsers, CommandUsers>();
    }
}