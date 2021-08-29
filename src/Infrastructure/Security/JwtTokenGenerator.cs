using Application.Interfaces;

namespace Infreastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        public string CreateToken(string username)
        {
            return "blabla";
        }
    }
}
