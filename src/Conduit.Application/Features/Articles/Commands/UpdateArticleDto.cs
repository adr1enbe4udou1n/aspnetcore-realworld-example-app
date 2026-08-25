using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Conduit.Application.Features.Articles.Commands;

public class UpdateArticleDto
{
    private Collection<string>? _tagList;

    [DisallowNull]
    public string? Title { get; set; }

    [DisallowNull]
    public string? Description { get; set; }

    [DisallowNull]
    public string? Body { get; set; }

#pragma warning disable CA2227
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DisallowNull]
    public Collection<string>? TagList
    {
        get => _tagList;
        set
        {
            TagListSpecified = true;
            _tagList = value;
        }
    }
#pragma warning restore CA2227

    [JsonIgnore]
    public bool TagListSpecified { get; private set; }
}
