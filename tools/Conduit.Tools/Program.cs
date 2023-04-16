using Conduit.Infrastructure;
using Conduit.Tools.Interfaces;
using Conduit.Tools.Seeders;

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

public partial class Program
{
    protected Program() { }
}