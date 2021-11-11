using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities;

public class Article : IHasTimestamps
{
    public int Id { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [Column(TypeName = "varchar(255)")]
    public string Title { get; set; } = null!;

    [Column(TypeName = "varchar(255)")]
    public string Slug { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<Comment> Comments { get; set; } = new();

    public List<ArticleTag> Tags { get; set; } = new();

    public List<ArticleFavorite> FavoredUsers { get; set; } = new();

    public bool IsFavoritedBy(User user)
    {
        return FavoredUsers.Any(f => f.UserId == user.Id);
    }
}