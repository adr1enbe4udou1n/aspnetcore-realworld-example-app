using Conduit.Domain.Interfaces;

namespace Conduit.Domain.Entities;

public class Comment : IAuditableEntity
{
    public int Id { get; private set; }

    public int ArticleId { get; set; }
    public virtual required Article Article { get; set; }

    public int AuthorId { get; set; }
    public virtual required User Author { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}