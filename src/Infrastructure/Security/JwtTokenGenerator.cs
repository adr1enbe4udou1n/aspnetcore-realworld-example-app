using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly ICurrentUser _currentUser;
    private readonly JwtOptions _jwtOptions;
    private readonly HttpContext? _httpContext;

    public JwtTokenGenerator(IOptionsMonitor<JwtOptions> options, ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _jwtOptions = options.CurrentValue;
        _httpContext = httpContextAccessor.HttpContext;
    }

    public string CreateToken(User user)
    {
        if (_jwtOptions.SecretKey == null)
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
                Expires = tokenDescriptor.Expires,
            }
        );

        return response;
    }
}