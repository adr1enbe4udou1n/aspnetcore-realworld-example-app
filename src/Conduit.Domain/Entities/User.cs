using System.ComponentModel.DataAnnotations;

using Conduit.Domain.Interfaces;

namespace Conduit.Domain.Entities;

public class User : IAuditableEntity
{
    private readonly List<Article> _articles = new();
    private readonly List<ArticleFavorite> _favoriteArticles = new();
    private readonly List<Comment> _comments = new();
    private readonly List<FollowerUser> _following = new();
    private readonly List<FollowerUser> _followers = new();

    public int Id { get; private set; }

    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Email { get; set; }

    [MaxLength(255)]
    public string? Password { get; set; }

    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? Image { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual IReadOnlyCollection<Article> Articles => _articles;

    public virtual IReadOnlyCollection<ArticleFavorite> FavoriteArticles => _favoriteArticles;

    public virtual IReadOnlyCollection<Comment> Comments => _comments;

    public virtual IReadOnlyCollection<FollowerUser> Following => _following;

    public virtual IReadOnlyCollection<FollowerUser> Followers => _followers;

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

    public void AddFollowing(params User[] users)
    {
        _following.AddRange(users.Select(x => new FollowerUser { Following = x, Follower = this }).ToList());
    }
}