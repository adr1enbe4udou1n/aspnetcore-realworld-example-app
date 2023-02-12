using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Infrastructure.Security;

public class CurrentUser : ICurrentUser
{
    private readonly AppDbContext _context;

    public User? User { get; private set; }

    public CurrentUser(AppDbContext context)
    {
        _context = context;
    }

    public async Task SetIdentifier(long identifier)
    {
        User = await _context.Users.Where(x => x.Id == identifier).SingleOrDefaultAsync();
    }

    public async Task LoadFollowing()
    {
        if (User != null)
        {
            await _context.Entry(User).Collection(u => u.Following).LoadAsync();
        }
    }

    public async Task LoadFavoriteArticles()
    {
        if (User != null)
        {
            await _context.Entry(User).Collection(u => u.FavoriteArticles).LoadAsync();
        }
    }
}