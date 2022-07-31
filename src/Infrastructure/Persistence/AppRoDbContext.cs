using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppRoDbContext : BaseDbContext, IAppRoDbContext
{

    public AppRoDbContext(DbContextOptions<AppRoDbContext> options) : base(options)
    {
    }
}