namespace Domain.Entities;

public class ArticleFavorite
{
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}