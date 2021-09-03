using System;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IHasAuthor
    {
        int AuthorId { get; set; }
        User Author { get; set; }
    }
}