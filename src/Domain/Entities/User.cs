using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class User : IHasTimestamps
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string Password { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<Article> Articles { get; set; } = new();

        public List<ArticleFavorite> FavoriteArticles { get; set; } = new();

        public List<Comment> Comments { get; set; } = new();

        public List<AuthorFollower> Following { get; set; } = new();

        public List<AuthorFollower> Followers { get; set; } = new();
    }
}