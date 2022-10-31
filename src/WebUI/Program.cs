using System.ComponentModel;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Filters;
using WebUI.Filters;
using WebUI.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddInfrastructure(builder.Configuration);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services
    .AddControllers(opt =>
    {
        opt.Filters.Add(typeof(ApiExceptionFilterAttribute));
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new WebUI.Converters.DateTimeConverter())
    );

builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("Bearer", null);

builder.Services.AddAuthorization(options =>
{
    var policy = new AuthorizationPolicyBuilder("Bearer");
    policy.RequireAuthenticatedUser();
    options.DefaultPolicy = policy.Build();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conduit API",
        Version = "1.0.0",
        Description = "Conduit API",
        Contact = new OpenApiContact
        {
            Name = "RealWorld",
            Url = new Uri("https://realworld.io"),
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT"),
        },
    });

    c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "Application.xml"));
    c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "WebUI.xml"));

    c.AddServer(new OpenApiServer
    {
        Url = "/api",
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT"
    });

    c.SupportNonNullableReferenceTypes();
    c.SchemaFilter<RequiredNotNullableSchemaFilter>();

    c.CustomSchemaIds(x => x.GetCustomAttributes(false)
        .OfType<DisplayNameAttribute>()
        .FirstOrDefault()?.DisplayName ?? x.Name.Replace("DTO", string.Empty)
    );

    c.TagActionsBy(y => new[]
    {
            y.GroupName ?? throw new InvalidOperationException()
    });

    c.DocInclusionPredicate((name, api) => true);

    c.OperationFilter<SecurityRequirementsOperationFilter>();

    c.DescribeAllParametersInCamelCase();
});

if (builder.Configuration.GetValue<bool>("Tracing:Enabled"))
{
    builder.Services.AddOpenTelemetryTracing(b =>
    {
        b
            .SetResourceBuilder(ResourceBuilder
                .CreateDefault()
                .AddService("ASPNET Core RealWorld"))
            .AddAspNetCoreInstrumentation()
            .AddNpgsql()
            .AddSource("ASPNET Core RealWorld")
            .AddJaegerExporter(o =>
            {
                o.AgentHost = builder.Configuration.GetValue<string>("Jaeger:Host");
                o.AgentPort = builder.Configuration.GetValue<int>("Jaeger:Port");
            });
    });
}

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

app.Map("/api", app =>
{
    app.UseRouting();

    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
    );

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
});

app.Run();

public partial class Program { }