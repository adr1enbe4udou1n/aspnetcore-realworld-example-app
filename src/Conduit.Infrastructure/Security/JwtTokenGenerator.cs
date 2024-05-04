using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Infrastructure.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Infrastructure.Security;

public class JwtTokenGenerator(IOptionsMonitor<JwtOptions> options, IHttpContextAccessor httpContextAccessor) : IJwtTokenGenerator
{
    public string CreateToken(User user)
    {
        if (options.CurrentValue.SecretKey is null)
        {
            throw new ArgumentException("You must set a JWT secret key");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = options.CurrentValue.Issuer,
            Audience = options.CurrentValue.Audience,
            Subject = new ClaimsIdentity(new Claim[] {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.CurrentValue.SecretKey)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var response = tokenHandler.WriteToken(token);

        httpContextAccessor.HttpContext?.Response.Cookies.Append(
            JwtBearerDefaults.AuthenticationScheme,
            response,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = tokenDescriptor.Expires,
            }
        );

        return response;
    }
}