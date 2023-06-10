using Conduit.Domain.Entities;

namespace Conduit.Application.Extensions;

public static class ArticleFilters
{
    public static IQueryable<Article> FilterByAuthor(
        this IQueryable<Article> source,
        string? author
    )
    {
        if (string.IsNullOrEmpty(author))
        {
            return source;
        }
        return source.Where(a => a.Author.Name == author);
    }

    public static IQueryable<Article> HasAuthorsFollowedBy(
        this IQueryable<Article> source,
        User user
    )
    {
        return source.Where(a => a.Author.Followers.Any(f => f.FollowerId == user.Id));
    }

    public static IQueryable<Article> FilterByTag(
        this IQueryable<Article> source,
        string? name
    )
    {
        if (!string.IsNullOrEmpty(name))
        {
            return source.Where(a => a.Tags.Any(t => t.Tag.Name == name));
        }
        return source;
    }

    public static IQueryable<Article> FilterByFavoritedBy(
        this IQueryable<Article> source,
        string? favoritedBy
    )
    {
        if (string.IsNullOrEmpty(favoritedBy))
        {
            return source;
        }
        return source.Where(a => a.FavoredUsers.Any(t => t.User.Name == favoritedBy));
    }
}