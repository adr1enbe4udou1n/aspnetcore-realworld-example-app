using Bogus;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Tools.Interfaces;

namespace Conduit.Tools.Seeders;

public class UsersSeeder(IAppDbContext context, IPasswordHasher passwordHasher) : ISeeder
{
    public async Task Run(CancellationToken cancellationToken)
    {
        var users = new Faker<User>()
            .RuleFor(m => m.Name, f => f.Person.FullName)
            .RuleFor(m => m.Email, f => f.Person.Email)
            .RuleFor(m => m.Bio, f => f.Lorem.Paragraphs(3))
            .RuleFor(m => m.Image, f => f.Internet.Avatar())
            .RuleFor(m => m.Password, passwordHasher.Hash("password"))
            .Generate(50);

        await context.Users.AddRangeAsync(users, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        users.ForEach(u =>
        {
            var f = new Faker();
            f.PickRandom(users, f.Random.Number(5))
                .ToList()
                .ForEach(follower =>
                {
                    u.AddFollower(follower);
                    context.Users.Update(u);
                });
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}