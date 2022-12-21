using Application.Tools.Interfaces;
using Application.Tools.Seeders;

using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Tools.Commands;

var builder = ConsoleApp.CreateBuilder(args);

builder.ConfigureServices((ctx, services) =>
{
    services.AddInfrastructure(ctx.Configuration)
        .AddScoped<UsersSeeder>()
        .AddScoped<ArticlesSeeder>()
        .AddScoped<IEnumerable<ISeeder>>(options => new List<ISeeder>
        {
                options.GetRequiredService<UsersSeeder>(),
                options.GetRequiredService<ArticlesSeeder>()
        });
});

var app = builder.Build();

app.AddSubCommands<SeederCommand>();

app.Run();

internal sealed partial class Program { }
