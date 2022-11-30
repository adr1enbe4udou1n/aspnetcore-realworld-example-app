using System.ComponentModel;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebUI.Filters;

namespace WebUI.OptionsSetup;

public class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
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

        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Application.xml"));
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "WebUI.xml"));

        options.AddServer(new OpenApiServer
        {
            Url = "/api",
        });

        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please insert JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT"
        });

        options.SupportNonNullableReferenceTypes();
        options.SchemaFilter<RequiredNotNullableSchemaFilter>();

        options.CustomSchemaIds(x => x.GetCustomAttributes(false)
            .OfType<DisplayNameAttribute>()
            .FirstOrDefault()?.DisplayName ?? x.Name.Replace("DTO", string.Empty)
        );

        options.TagActionsBy(y => new[]
        {
            y.GroupName ?? throw new InvalidOperationException()
        });

        options.DocInclusionPredicate((name, api) => true);

        options.OperationFilter<SecurityRequirementsOperationFilter>();

        options.DescribeAllParametersInCamelCase();
    }
}