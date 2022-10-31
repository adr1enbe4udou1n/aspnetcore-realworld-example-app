using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities;

public class Article : IHasTimestamps
{
    private readonly List<Comment> _comments = new();
    private readonly List<ArticleTag> _tags = new();
    private readonly List<ArticleFavorite> _favoredUsers = new();

    public int Id { get; private set; }

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

    public IReadOnlyCollection<Comment> Comments => _comments;

    public IReadOnlyCollection<ArticleTag> Tags => _tags;

    public IReadOnlyCollection<ArticleFavorite> FavoredUsers => _favoredUsers;

    public bool IsFavoritedBy(User user)
    {
        return FavoredUsers.Any(f => f.UserId == user.Id);
    }
    public void Favorite(User user)
    {
        if (!IsFavoritedBy(user))
        {
            _favoredUsers.Add(new ArticleFavorite { User = user });
        }
    }

    public void Unfavorite(User user)
    {
        if (IsFavoritedBy(user))
        {
            _favoredUsers.RemoveAll(x => x.UserId == user.Id);
        }
    }

    public void AddTags(List<Tag> existingTags, List<string> newTags)
    {
        _tags.AddRange(
            newTags
                .Where(x => !String.IsNullOrEmpty(x))
                .Distinct()
                .Select(x =>
                {
                    var tag = existingTags.FirstOrDefault(t => t.Name == x);

                    return new ArticleTag
                    {
                        Tag = tag == null ? new Tag { Name = x } : tag
                    };
                })
                .ToList()
        );
    }
}