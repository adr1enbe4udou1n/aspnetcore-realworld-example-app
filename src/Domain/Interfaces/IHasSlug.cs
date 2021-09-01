using System;

namespace Domain.Interfaces
{
    public interface IHasSlug
    {
        string Slug { get; set; }

        string GetSlugSource();
    }
}