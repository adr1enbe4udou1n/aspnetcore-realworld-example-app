using System.Collections.Generic;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string CreateToken(User user);

        IDictionary<string, object> DecodeToken(string token);
    }
}