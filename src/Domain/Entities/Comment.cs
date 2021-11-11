using Domain.Interfaces;

namespace Domain.Entities;

public class Comment : IHasTimestamps
{
    public int Id { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = new();

    public int AuthorId { get; set; }
    public User Author { get; set; } = new();

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}