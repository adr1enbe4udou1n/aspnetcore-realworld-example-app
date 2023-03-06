using System.ComponentModel.DataAnnotations;

namespace Conduit.Domain.Entities;

public class Tag
{
    private readonly List<ArticleTag> _articles = new();

    public int Id { get; private set; }

    [MaxLength(255)]
    public required string Name { get; set; }

    public virtual IReadOnlyCollection<ArticleTag> Articles => _articles;
}