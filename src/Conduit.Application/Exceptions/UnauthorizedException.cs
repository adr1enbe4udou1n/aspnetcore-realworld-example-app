using System.Runtime.Serialization;

namespace Application.Exceptions;

[Serializable]
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Not authenticated")
    {
    }

    protected UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}