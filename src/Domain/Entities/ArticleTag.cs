using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ArticleTag
    {
        [Required]
        public int ArticleId { get; set; }
        public Article Article { get; set; }

        [Required]
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}