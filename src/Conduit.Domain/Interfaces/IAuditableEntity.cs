namespace Conduit.Domain.Interfaces;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    DateTime UpdatedAt { get; set; }
}