using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities;

public class User : IHasTimestamps
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(255)")]
    public string Name { get; set; } = null!;

    [Column(TypeName = "varchar(255)")]
    public string Email { get; set; } = null!;

    [Column(TypeName = "varchar(255)")]
    public string? Password { get; set; }

    public string? Bio { get; set; }

    [Column(TypeName = "varchar(255)")]
    public string? Image { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<Article> Articles { get; set; } = new();

    public List<ArticleFavorite> FavoriteArticles { get; set; } = new();

    public List<Comment> Comments { get; set; } = new();

    public List<FollowerUser> Following { get; set; } = new();

    public List<FollowerUser> Followers { get; set; } = new();

    public bool IsFollowing(User user)
    {
        return Following.Any(f => f.FollowingId == user.Id);
    }

    public bool IsFollowedBy(User user)
    {
        return Followers.Any(f => f.FollowerId == user.Id);
    }

    public bool HasFavorite(Article article)
    {
        return FavoriteArticles.Any(f => f.ArticleId == article.Id);
    }
}