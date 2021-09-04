using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;

namespace Application.Extensions
{
    public static class ArticleFilters
    {
        public static IQueryable<Article> FilterByAuthor(
            this IQueryable<Article> source,
            string author
        )
        {
            if (!string.IsNullOrEmpty(author))
            {
                return source.Where(a => a.Author.Name.Contains(author));
            }
            return source;
        }

        public static IQueryable<Article> FilterByAuthors(
            this IQueryable<Article> source,
            IEnumerable<int> ids
        )
        {
            return source.Where(a => ids.Contains(a.AuthorId));
        }

        public static IQueryable<Article> FilterByTag(
            this IQueryable<Article> source,
            string name
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
            string favoritedBy
        )
        {
            if (!string.IsNullOrEmpty(favoritedBy))
            {
                return source.Where(a => a.FavoredUsers.Any(t => t.User.Name.Contains(favoritedBy)));
            }
            return source;
        }
    }
}