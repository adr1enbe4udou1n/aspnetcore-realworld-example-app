using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Article : IHasTimestamps, IHasSlug
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Slug { get; set; }

        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        public int AuthorId { get; set; }
        public User Author { get; set; }

        public List<Comment> Comments { get; set; } = new();

        public List<ArticleTag> Tags { get; set; } = new();

        public List<ArticleFavorite> FavoredUsers { get; set; } = new();

        public string GetSlugSource() => Title;
    }
}