using System.Collections.ObjectModel;

using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;

namespace Conduit.Application.Features.Articles.Queries;

public class ArticleDto
{
    public required string Title { get; set; }

    public required string Slug { get; set; }

    public required string Description { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

#pragma warning disable CA2227
    public Collection<string> TagList { get; set; } = [];
#pragma warning restore CA2227

    public required ProfileDto Author { get; set; }

    public bool Favorited { get; set; }

    public int FavoritesCount { get; set; }
}


public static class ArticleMapper
{
    public static ArticleDto Map(this Article article, User? currentUser)
    {
        return new()
        {
            Slug = article.Slug,
            Title = article.Title,
            Description = article.Description,
            Body = article.Body,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            Favorited = currentUser != null && currentUser.HasFavorite(article),
            FavoritesCount = article.FavoredUsers.Count,
            Author = article.Author.MapToProfile(currentUser),
            TagList = new Collection<string>(article.Tags.Select(t => t.Tag.Name).OrderBy(t => t).ToList())
        };
    }
}