namespace Application.Tools.Interfaces;

public interface ISeeder
{
    Task Run(CancellationToken cancellationToken);
}