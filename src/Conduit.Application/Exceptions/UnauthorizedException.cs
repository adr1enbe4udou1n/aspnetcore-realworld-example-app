namespace Conduit.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Not authenticated")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}