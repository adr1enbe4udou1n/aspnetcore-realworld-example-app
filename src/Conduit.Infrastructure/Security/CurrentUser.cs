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
        User = await _context.Users
            .Include(x => x.Following)
            .Include(x => x.FavoriteArticles)
            .Where(x => x.Id == identifier).SingleOrDefaultAsync();
    }
}