using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ArticleFavorite
    {
        [Required]
        public int ArticleId { get; set; }
        public Article Article { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
