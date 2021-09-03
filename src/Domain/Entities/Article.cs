using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Article : IHasTimestamps, IHasSlug, IHasAuthor
    {
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; }
        public User Author { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Slug { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<Comment> Comments { get; set; } = new();

        public List<ArticleTag> Tags { get; set; } = new();

        public List<ArticleFavorite> FavoredUsers { get; set; } = new();

        public string GetSlugSource() => Title;
    }
}