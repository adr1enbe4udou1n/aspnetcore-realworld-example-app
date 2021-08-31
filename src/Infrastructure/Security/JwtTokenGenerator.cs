using System;
using System.Collections.Generic;
using Application.Interfaces;
using Domain.Entities;
using JWT.Algorithms;
using JWT.Builder;

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
            return JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_secret)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                .AddClaim(ClaimName.FullName, user.Name)
                .AddClaim(ClaimName.JwtId, user.Id)
                .AddClaim(ClaimName.VerifiedEmail, user.Email)
                .Encode();
        }

        public IDictionary<string, object> DecodeToken(string token)
        {
            return JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_secret)
                .MustVerifySignature()
                .Decode<IDictionary<string, object>>(token);
        }
    }
}
