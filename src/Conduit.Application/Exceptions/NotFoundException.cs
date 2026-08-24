namespace Conduit.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : this("resource")
    {
    }

    public NotFoundException(string resource) : base($"{resource} not found")
    {
        Resource = resource;
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        Resource = "resource";
    }

    public string Resource { get; }
}