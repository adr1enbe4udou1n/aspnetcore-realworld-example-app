using System.ComponentModel;

using Conduit.Presentation.Filters;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Conduit.Presentation.OptionsSetup;

public class SwaggerGenOptionsSetup(IConfiguration configuration) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", configuration.GetSection("OpenApiInfo").Get<OpenApiInfo>());
        options.DocumentFilter<PathPrefixDocumentFilter>("api");

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

        options.DocInclusionPredicate((name, api) => true);

        options.OperationFilter<SecurityRequirementsOperationFilter>();

        options.DescribeAllParametersInCamelCase();
    }
}