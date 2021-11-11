namespace Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Not authenticated")
    {
    }
}