using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateToken(User user);

    IDictionary<string, string> DecodeToken(string token);

    Task SetCurrentUserFromToken(string token);
}
