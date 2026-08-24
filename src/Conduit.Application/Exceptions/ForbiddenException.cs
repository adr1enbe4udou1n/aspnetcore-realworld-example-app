namespace Conduit.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : this("resource")
    {
    }

    public ForbiddenException(string resource) : base($"{resource} forbidden")
    {
        Resource = resource;
    }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
        Resource = "resource";
    }

    public string Resource { get; }
}