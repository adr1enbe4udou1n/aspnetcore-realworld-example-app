using Conduit.Domain.Entities;

namespace Conduit.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateToken(User user);
}