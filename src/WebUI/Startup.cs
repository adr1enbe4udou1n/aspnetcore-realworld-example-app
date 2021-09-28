using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebUI.Converters;
using WebUI.Filters;
using WebUI.Handlers;

namespace WebUI
{
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

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebUI.xml");
                c.IncludeXmlComments(filePath);

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
                    .FirstOrDefault()?.DisplayName ?? x.Name
                );

                c.TagActionsBy(y => new[]
                {
                    y.GroupName ?? throw new InvalidOperationException()
                });

                c.DocInclusionPredicate((name, api) => true);

                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conduit v1"));

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
}
