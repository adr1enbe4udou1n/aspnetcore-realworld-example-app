using Conduit.Domain.Entities;

namespace Conduit.Application.Interfaces;

public interface ICurrentUser
{
    User? User { get; }

    Task SetIdentifier(long identifier);
}