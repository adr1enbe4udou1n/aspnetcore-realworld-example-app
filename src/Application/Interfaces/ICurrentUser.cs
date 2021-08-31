using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICurrentUser
    {
        User User { get; }

        string Token { get; }

        public bool IsAuthenticated { get; }

        public Task SetToken(string token);

        public Task Fresh();
    }
}