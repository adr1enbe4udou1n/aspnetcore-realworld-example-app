using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Tools.Interfaces;
using Application.Tools.Seeders;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Bullseye.Targets;

namespace Application.Tools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();

            Target("fresh", "Wipe all data from database",
                async () =>
                {
                    var databaseManager = scope.ServiceProvider.GetRequiredService<DatabaseManager>();
                    await databaseManager.Reset();
                });

            Target("seed", "Seed data to database", DependsOn("fresh"),
                async () =>
                {
                    var token = new CancellationToken();

                    foreach (var seeder in scope.ServiceProvider.GetRequiredService<IEnumerable<ISeeder>>())
                    {
                        await seeder.Run(token);
                    }
                });

            await RunTargetsAndExitAsync(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config => config.AddJsonFile("appsettings.json", true, true))
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddInfrastructure(_.Configuration)
                        .AddScoped<DatabaseManager>()
                        .AddScoped<UsersSeeder>()
                        .AddScoped<ArticlesSeeder>()
                        .AddScoped<IEnumerable<ISeeder>>(options => new List<ISeeder>
                        {
                            options.GetRequiredService<UsersSeeder>(),
                            options.GetRequiredService<ArticlesSeeder>()
                        });
                });
    }
}
