using System.Reflection;
using Application.Behaviors;
using Application.Features.Auth.Commands;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services.AddAutoMapper(Assembly.GetExecutingAssembly())
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(DbTransactionBehavior<,>));
        }
    }
}
