namespace Conduit.Tools.Interfaces;

public interface ISeeder
{
    Task Run(CancellationToken cancellationToken);
}