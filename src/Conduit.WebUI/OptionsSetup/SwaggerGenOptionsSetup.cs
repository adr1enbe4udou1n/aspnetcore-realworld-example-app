using System.ComponentModel;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Conduit.WebUI.Filters;

namespace Conduit.WebUI.OptionsSetup;

public class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IConfiguration _configuration;

    public SwaggerGenOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", _configuration.GetSection("OpenApiInfo").Get<OpenApiInfo>());

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
            .FirstOrDefault()?.DisplayName ?? x.Name.Replace("Dto", string.Empty)
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