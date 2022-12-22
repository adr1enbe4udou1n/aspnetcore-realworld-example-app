using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateToken(User user);
}