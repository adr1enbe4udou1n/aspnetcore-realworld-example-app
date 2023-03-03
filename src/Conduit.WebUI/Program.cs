using Conduit.Application;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Persistence;
using Conduit.Presentation;
using Conduit.WebUI.OptionsSetup;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

builder.Services
    .ConfigureOptions<JwtOptionsSetup>()
    .ConfigureOptions<JwtBearerOptionsSetup>()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .ConfigureOptions<TracerOptionsSetup>()
    .ConfigureOptions<TracerProviderBuilderSetup>()
    .AddOpenTelemetry()
    .WithTracing();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();

public partial class Program
{
    protected Program() { }
}