using Application.Tools.Interfaces;
using Application.Tools.Seeders;
using Cocona;

using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Tools.Commands;

var builder = CoconaApp.CreateBuilder();

builder.Services.AddInfrastructure(builder.Configuration)
    .AddScoped<UsersSeeder>()
    .AddScoped<ArticlesSeeder>()
    .AddScoped<IEnumerable<ISeeder>>(options => new List<ISeeder>
    {
            options.GetRequiredService<UsersSeeder>(),
            options.GetRequiredService<ArticlesSeeder>()
    });

var app = builder.Build();

app.AddSubCommand("db", x =>
{
    x.AddCommands<SeederCommand>();
})
.WithDescription("DB related commands");

app.Run();