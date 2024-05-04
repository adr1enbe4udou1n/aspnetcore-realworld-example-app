using Conduit.Application.Interfaces;

using Slugify;

namespace Conduit.Infrastructure.Security;

public class Slugifier(ISlugHelper slugHelper) : ISlugifier
{
    private readonly ISlugHelper _slugHelper = slugHelper;

    public string Generate(string text)
    {
        return _slugHelper.GenerateSlug(text);
    }
}