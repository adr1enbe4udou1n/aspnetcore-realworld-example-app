using Conduit.Application.Interfaces;

using Slugify;

namespace Conduit.Infrastructure.Security;

public class Slugifier(ISlugHelper slugHelper) : ISlugifier
{
    public string Generate(string text)
    {
        return slugHelper.GenerateSlug(text);
    }
}