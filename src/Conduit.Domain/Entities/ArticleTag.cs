namespace Conduit.Domain.Entities;

public class ArticleTag
{
    public int ArticleId { get; set; }
    public virtual required Article Article { get; set; }

    public int TagId { get; set; }
    public virtual required Tag Tag { get; set; }
}