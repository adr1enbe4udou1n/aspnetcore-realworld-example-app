using System.ComponentModel.DataAnnotations;

using Conduit.Domain.Interfaces;

namespace Conduit.Domain.Entities;

public class Article : IAuditableEntity
{
    private readonly List<Comment> _comments = new();
    private readonly List<ArticleTag> _tags = new();
    private readonly List<ArticleFavorite> _favoredUsers = new();

    public int Id { get; private set; }

    public int AuthorId { get; set; }

    public required virtual User Author { get; set; }


    [MaxLength(255)]
    public required string Title { get; set; }


    [MaxLength(255)]
    public required string Slug { get; set; }

    public required string Description { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual IReadOnlyCollection<Comment> Comments => _comments;

    public virtual IReadOnlyCollection<ArticleTag> Tags => _tags;

    public virtual IReadOnlyCollection<ArticleFavorite> FavoredUsers => _favoredUsers;

    public bool IsFavoritedBy(User user)
    {
        return FavoredUsers.Any(f => f.UserId == user.Id);
    }

    public void AddComment(Comment comment)
    {
        _comments.Add(comment);
    }

    public void Favorite(User user)
    {
        if (!IsFavoritedBy(user))
        {
            _favoredUsers.Add(new ArticleFavorite { User = user, Article = this });
        }
    }

    public void Unfavorite(User user)
    {
        if (IsFavoritedBy(user))
        {
            _favoredUsers.RemoveAll(x => x.UserId == user.Id);
        }
    }

    public void AddTag(Tag tag)
    {
        _tags.Add(new ArticleTag { Tag = tag, Article = this });
    }

    public void AddTags(IEnumerable<Tag> existingTags, params string[] newTags)
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
                        Tag = tag ?? new Tag { Name = x },
                        Article = this
                    };
                })
                .ToList()
        );
    }
}