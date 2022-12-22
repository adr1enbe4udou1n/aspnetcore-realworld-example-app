using Conduit.Application.Interfaces;

namespace Conduit.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Check(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}