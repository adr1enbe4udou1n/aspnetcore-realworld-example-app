using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.OptionsSetup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers(opt =>
    {
        opt.UseRoutePrefix("api");
        opt.Filters.Add(typeof(ApiExceptionFilterAttribute));
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new WebUI.Converters.DateTimeConverter())
    );

builder.Services
    .ConfigureOptions<JwtOptionsSetup>()
    .ConfigureOptions<JwtBearerOptionsSetup>()
    .ConfigureOptions<SwaggerGenOptionsSetup>()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

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

app.Run();

public partial class Program { }