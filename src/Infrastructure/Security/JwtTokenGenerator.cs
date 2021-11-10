using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Infrastructure.Settings;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly ICurrentUser _currentUser;
    private readonly SymmetricSecurityKey _secret;

    public JwtTokenGenerator(IOptionsMonitor<JwtOptions> options, ICurrentUser currentUser)
    {
        if (options.CurrentValue.SecretKey == null)
        {
            throw new ArgumentNullException("You must set a JWT secret key");
        }

        _secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.CurrentValue.SecretKey));
        _currentUser = currentUser;
    }

    public string CreateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim("id", user.Id.ToString(CultureInfo.InvariantCulture)),
                    new Claim("name", user.Name),
                    new Claim("email", user.Email)
                }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                _secret, SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public IDictionary<string, string> DecodeToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _secret,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        return jwtToken.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(group => group.Key, group => group.Last().Value);
    }

    public async Task SetCurrentUserFromToken(string token)
    {
        var userId = long.Parse(DecodeToken(token)["id"], CultureInfo.InvariantCulture);

        await _currentUser.SetIdentifier(userId);
    }
}
