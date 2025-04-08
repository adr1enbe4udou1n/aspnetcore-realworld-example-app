using System.Reflection;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Comments.Commands;
using Conduit.Application.Features.Comments.Queries;
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
            .AddScoped<IQueryTags, QueryTags>()
            .AddScoped<IQueryProfiles, QueryProfiles>()
            .AddScoped<ICommandProfiles, CommandProfiles>()
            .AddScoped<IQueryUsers, QueryUsers>()
            .AddScoped<ICommandUsers, CommandUsers>()
            .AddScoped<ICommandComments, CommandComments>()
            .AddScoped<ICommandArticles, CommandArticles>()
            .AddScoped<IQueryArticles, QueryArticles>()
            .AddScoped<IQueryComments, QueryComments>();
    }
}