namespace Conduit.Domain.Entities;

public class ArticleFavorite
{
    public int ArticleId { get; set; }
    public virtual required Article Article { get; set; }

    public int UserId { get; set; }
    public virtual required User User { get; set; }
}