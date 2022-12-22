namespace Conduit.Domain.Entities;

public class ArticleFavorite
{
    public int ArticleId { get; set; }
    public virtual Article Article { get; set; } = null!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
}