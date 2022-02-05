namespace Application.Interfaces;

public interface IAppDbContextFactory
{
    IAppDbContext CreateDbContext();
}