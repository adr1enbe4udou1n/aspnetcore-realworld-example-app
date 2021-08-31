using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Tools.Interfaces;
using Bogus;
using Domain.Entities;

namespace Application.Tools.Seeders
{
    public class UsersSeeder : ISeeder
    {
        private IAppDbContext _context;
        private IPasswordHasher _passwordHasher;

        public UsersSeeder(IAppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(new User
            {
                Name = "admin",
                Email = "admin@example.com",
                Password = _passwordHasher.Hash("password"),
            });

            var user = new Faker<User>()
                .RuleFor(m => m.Name, f => f.Person.FullName)
                .RuleFor(m => m.Email, f => f.Person.Email)
                .RuleFor(m => m.Password, _passwordHasher.Hash("password"));

            await _context.Users.AddRangeAsync(user.Generate(10));

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}