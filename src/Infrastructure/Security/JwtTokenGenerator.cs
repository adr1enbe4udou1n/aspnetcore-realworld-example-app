using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _secret;

        public JwtTokenGenerator(string secret)
        {
            _secret = secret;
        }

        public string CreateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name),
                    new Claim("email", user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public IDictionary<string, string> DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(group => group.Key, group => group.Last().Value);
        }
    }
}
