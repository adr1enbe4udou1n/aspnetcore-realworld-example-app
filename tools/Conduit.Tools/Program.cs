using Conduit.Infrastructure;
using Conduit.Tools.Interfaces;
using Conduit.Tools.Seeders;

using ConsoleAppFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Tools.Commands;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddInfrastructure(configuration)
    .AddSingleton<IConfiguration>(configuration)
    .AddScoped<UsersSeeder>()
    .AddScoped<ArticlesSeeder>()
    .AddScoped<IEnumerable<ISeeder>>(options =>
    [
        options.GetRequiredService<UsersSeeder>(),
        options.GetRequiredService<ArticlesSeeder>()
    ]);

using var serviceProvider = services.BuildServiceProvider();
ConsoleApp.ServiceProvider = serviceProvider;

var app = ConsoleApp.Create();

app.Add<SeederCommand>("db");

await app.RunAsync(args);

public partial class Program
{
    protected Program() { }
}