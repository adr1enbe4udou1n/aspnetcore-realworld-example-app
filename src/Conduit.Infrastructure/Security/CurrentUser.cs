using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Infrastructure.Security;

public class CurrentUser(AppDbContext context) : ICurrentUser
{
    private readonly AppDbContext _context = context;

    public User? User { get; private set; }

    public async Task SetIdentifier(long identifier)
    {
        User = await _context.Users
            .Include(x => x.Following)
            .Include(x => x.FavoriteArticles)
            .Where(x => x.Id == identifier).SingleOrDefaultAsync();
    }
}