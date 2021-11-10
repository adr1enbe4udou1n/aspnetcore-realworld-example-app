using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Domain.Interfaces;

namespace Domain.Entities;

public class Article : IHasTimestamps
{
    public int Id { get; set; }

    [Required]
    public int AuthorId { get; set; }
    public User Author { get; set; } = new();

    [Required]
    [Column(TypeName = "varchar(255)")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(255)")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

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
