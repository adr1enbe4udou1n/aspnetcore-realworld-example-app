using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Tools.Interfaces;
using Bogus;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

            var users = new Faker<User>()
                .RuleFor(m => m.Name, f => f.Person.FullName)
                .RuleFor(m => m.Email, f => f.Person.Email)
                .RuleFor(m => m.Password, _passwordHasher.Hash("password"))
                .Generate(50);

            await _context.Users.AddRangeAsync(users, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            users.ForEach(u =>
            {
                u.Followers = new Faker().PickRandom(users, new Faker().Random.Number(5))
                    .Select(u => new FollowerUser { FollowerId = u.Id })
                    .ToList();
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}