using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IAppDbContextFactory
{
    private IDbContextFactory<AppDbContext> _contextFactory;

    public AppDbContextFactory(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    IAppDbContext IAppDbContextFactory.CreateDbContext()
    {
        return _contextFactory.CreateDbContext();
    }
}