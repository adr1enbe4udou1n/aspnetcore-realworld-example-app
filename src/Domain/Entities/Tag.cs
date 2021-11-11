using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Tag
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(255)")]
    public string Name { get; set; } = null!;

    public List<ArticleTag> Articles { get; set; } = new();
}