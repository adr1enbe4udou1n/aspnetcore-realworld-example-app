namespace Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Access forbidden")
    {
    }
}
