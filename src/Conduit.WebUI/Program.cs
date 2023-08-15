using System.Globalization;

using Conduit.Application;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Persistence;
using Conduit.Presentation;
using Conduit.WebUI.OptionsSetup;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

builder.Services
    .ConfigureOptions<JwtOptionsSetup>()
    .ConfigureOptions<JwtBearerOptionsSetup>()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services
        .ConfigureOptions<TracerOptionsSetup>()
        .ConfigureOptions<TracerProviderBuilderSetup>()
        .AddOpenTelemetry()
        .WithTracing();

    builder.Host.UseSerilog((context, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));
}

var app = builder.Build();

app.MapReverseProxy();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSerilogRequestLogging();
}

app.UseSwagger(c =>
{
    c.RouteTemplate = "/api/{documentName}/docs.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/docs.json", "Conduit v1");
    c.RoutePrefix = "api";
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}