using System.ComponentModel.DataAnnotations;

using Conduit.Domain.Interfaces;

namespace Conduit.Domain.Entities;

public class User : IAuditableEntity
{
    private readonly List<Article> _articles = [];
    private readonly List<ArticleFavorite> _favoriteArticles = [];
    private readonly List<Comment> _comments = [];
    private readonly List<FollowerUser> _following = [];
    private readonly List<FollowerUser> _followers = [];

    public int Id { get; set; }

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

    public void AddFollower(User user)
    {
        _followers.Add(new FollowerUser { Following = this, Follower = user });
    }

    public void RemoveFollower(User user)
    {
        var follower = Followers.FirstOrDefault(f => f.FollowerId == user.Id);

        if (follower is not null)
        {
            _followers.Remove(follower);
        }
    }
}