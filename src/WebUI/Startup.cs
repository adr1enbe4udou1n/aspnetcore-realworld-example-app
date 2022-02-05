using System.ComponentModel;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebUI.Filters;
using WebUI.Handlers;

namespace WebUI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructure(Configuration);

        services.AddRouting(options => options.LowercaseUrls = true);

        services
            .AddControllers(opt =>
            {
                opt.Filters.Add(typeof(ApiExceptionFilterAttribute));
            })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new Converters.DateTimeConverter())
            );

        services.AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("Bearer", null);

        services.AddAuthorization(options =>
        {
            var policy = new AuthorizationPolicyBuilder("Bearer");
            policy.RequireAuthenticatedUser();
            options.DefaultPolicy = policy.Build();
        });

        services.AddSwaggerGen(c =>
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

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT"
            });

            c.SupportNonNullableReferenceTypes();

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

        services.AddOpenTelemetryTracing(b =>
        {
            b
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService("ASPNET Core RealWorld"))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddNpgsql()
                .AddSource("ASPNET Core RealWorld")
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = Configuration.GetValue<string>("Jaeger:Host");
                    o.AgentPort = Configuration.GetValue<int>("Jaeger:Port");
                });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
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
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });
    }
}