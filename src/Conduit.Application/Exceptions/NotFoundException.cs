using System.Runtime.Serialization;

namespace Application.Exceptions;

[Serializable]
public class NotFoundException : Exception
{
    public NotFoundException() : base("Resource not found")
    {
    }

    protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}