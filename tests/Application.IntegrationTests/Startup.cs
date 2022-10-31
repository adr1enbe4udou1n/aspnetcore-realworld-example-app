using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

public class Startup
{
    public string ConnectionString { get; private set; }

    public ConduitApiApplicationFactory Application { get; private set; }

    public IAppDbContext Context { get; private set; }

    public Startup()
    {
        ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=5434;User Id=main;Password=main;Database=main;";

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", ConnectionString);
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "super secret key");

        Application = new ConduitApiApplicationFactory();

        var scope = Application.Services.CreateScope();

        Context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        Context.Database.Migrate();
    }
}