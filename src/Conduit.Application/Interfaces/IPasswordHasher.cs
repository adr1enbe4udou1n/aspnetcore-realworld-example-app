namespace Conduit.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Check(string password, string hash);
}