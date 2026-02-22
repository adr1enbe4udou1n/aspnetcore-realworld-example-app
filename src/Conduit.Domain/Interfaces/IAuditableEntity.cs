namespace Conduit.Domain.Interfaces;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }

    DateTime UpdatedAt { get; }
}