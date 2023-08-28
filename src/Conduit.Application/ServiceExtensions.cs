using System.Reflection;

using Conduit.Application.Behaviors;

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
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(CommandBehavior<,>));
    }
}