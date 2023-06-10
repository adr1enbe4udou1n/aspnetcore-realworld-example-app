using System.ComponentModel;

using Conduit.Presentation.Filters;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.OptionsSetup;

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
        options.DocumentFilter<PathPrefixDocumentFilter>("api");

        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Conduit.Application.xml"));
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Conduit.Presentation.xml"));

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
            .FirstOrDefault()?.DisplayName ?? x.Name.Replace("Dto", string.Empty, StringComparison.InvariantCultureIgnoreCase)
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