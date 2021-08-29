using System.Threading.Tasks;
using Application.Interfaces;

namespace Infreastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return "blabla";
        }
    }
}
