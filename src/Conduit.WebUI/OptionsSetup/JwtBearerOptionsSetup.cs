using System.Globalization;
using System.Security.Claims;
using System.Text;

using Conduit.Application.Interfaces;
using Conduit.Infrastructure.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.WebUI.OptionsSetup;

public class JwtBearerOptionsSetup : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;

    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.TokenValidationParameters.ValidIssuer = _jwtOptions.Issuer;
        options.TokenValidationParameters.ValidAudience = _jwtOptions.Audience;
        options.TokenValidationParameters.IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey!));

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
                var userId = context.Principal?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (userId != null)
                {
                    await currentUser.SetIdentifier(Convert.ToInt32(userId, CultureInfo.InvariantCulture));
                }

                if (currentUser.User is null)
                {
                    context.Fail("User unknown.");
                }
            },
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ")[^1]
                    ?? context.Request.Cookies[JwtBearerDefaults.AuthenticationScheme];

                return Task.CompletedTask;
            }
        };
    }
}