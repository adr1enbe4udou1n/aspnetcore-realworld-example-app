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

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;
    private readonly HttpContext? _httpContext;

    public JwtTokenGenerator(IOptionsMonitor<JwtOptions> options, IHttpContextAccessor httpContextAccessor)
    {
        _jwtOptions = options.CurrentValue;
        _httpContext = httpContextAccessor.HttpContext;
    }

    public string CreateToken(User user)
    {
        if (_jwtOptions.SecretKey is null)
        {
            throw new ArgumentException("You must set a JWT secret key");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Subject = new ClaimsIdentity(new Claim[] {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var response = tokenHandler.WriteToken(token);

        _httpContext?.Response.Cookies.Append(
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