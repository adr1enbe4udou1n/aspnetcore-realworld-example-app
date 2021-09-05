using System.Threading;
using System.Threading.Tasks;

namespace Application.Tools.Interfaces
{
    public interface ISeeder
    {
        Task Run(CancellationToken cancellationToken);
    }
}
