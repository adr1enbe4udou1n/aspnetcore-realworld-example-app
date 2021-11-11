using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;

namespace Domain.Entities;

public class Comment : IHasTimestamps
{
    public int Id { get; set; }

    [Required]
    public int ArticleId { get; set; }
    public Article Article { get; set; } = new();

    [Required]
    public int AuthorId { get; set; }
    public User Author { get; set; } = new();

    [Required]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
