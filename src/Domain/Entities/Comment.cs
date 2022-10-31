using Domain.Interfaces;

namespace Domain.Entities;

public class Comment : IHasTimestamps
{
    public int Id { get; private set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}