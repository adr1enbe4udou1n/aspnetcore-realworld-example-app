namespace Domain.Entities;

public class ArticleFavorite
{
    public int ArticleId { get; set; }
    public Article Article { get; set; } = new();

    public int UserId { get; set; }
    public User User { get; set; } = new();
}