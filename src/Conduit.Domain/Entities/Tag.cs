using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Tag
{
    private readonly List<ArticleTag> _articles = new();

    public int Id { get; private set; }

    [Column(TypeName = "varchar(255)")]
    public string Name { get; set; } = null!;

    public virtual IReadOnlyCollection<ArticleTag> Articles => _articles;
}