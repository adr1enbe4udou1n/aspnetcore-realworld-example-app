namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string CreateToken(string username);
    }
}