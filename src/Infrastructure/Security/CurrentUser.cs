using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Infrastructure.Security
{
    public class CurrentUser : ICurrentUser
    {
        private IAppDbContext _context;
        private IJwtTokenGenerator _jwtTokenGenerator;

        public string Token { get; private set; }

        public User User { get; private set; }

        public bool IsAuthenticated { get => User != null; }

        public CurrentUser(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task SetToken(string token)
        {
            Token = token;
            await Fresh();
        }

        public async Task Fresh()
        {
            var payload = _jwtTokenGenerator.DecodeToken(Token);

            User = await _context.Users.Where(x => x.Id == (long)payload["jti"]).SingleOrDefaultAsync();
        }
    }
}