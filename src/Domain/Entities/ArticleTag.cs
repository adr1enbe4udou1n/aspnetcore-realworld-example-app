namespace Domain.Entities;

public class ArticleTag
{
    public int ArticleId { get; set; }
    public virtual Article Article { get; set; } = null!;

    public int TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;
}