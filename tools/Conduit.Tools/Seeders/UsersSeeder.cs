using Bogus;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Tools.Interfaces;

namespace Conduit.Tools.Seeders;

public class UsersSeeder : ISeeder
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UsersSeeder(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var users = new Faker<User>()
            .RuleFor(m => m.Name, f => f.Person.FullName)
            .RuleFor(m => m.Email, f => f.Person.Email)
            .RuleFor(m => m.Bio, f => f.Lorem.Paragraphs(3))
            .RuleFor(m => m.Image, f => f.Internet.Avatar())
            .RuleFor(m => m.Password, _passwordHasher.Hash("password"))
            .Generate(50);

        await _context.Users.AddRangeAsync(users, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        users.ForEach(u =>
        {
            var f = new Faker();
            f.PickRandom(users, f.Random.Number(5))
                .ToList()
                .ForEach(follower => u.Follow(follower));
        });
        await _context.SaveChangesAsync(cancellationToken);
    }
}