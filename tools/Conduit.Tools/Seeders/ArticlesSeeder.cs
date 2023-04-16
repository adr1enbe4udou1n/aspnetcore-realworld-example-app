using Bogus;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Tools.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Tools.Seeders;

public class ArticlesSeeder : ISeeder
{
    private readonly IAppDbContext _context;
    private readonly ISlugifier _slugifier;

    public ArticlesSeeder(IAppDbContext context, ISlugifier slugifier)
    {
        _context = context;
        _slugifier = slugifier;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var users = await _context.Users.ToListAsync(cancellationToken);

        var tags = new Faker<Tag>()
            .RuleFor(a => a.Name, f => $"{f.Lorem.Word()}{f.UniqueIndex % 10}".TrimEnd('0'))
            .Generate(30);

        await _context.Tags.AddRangeAsync(tags, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);

        var articles = new Faker<Article>()
            .RuleFor(a => a.Title, f => f.Lorem.Sentence().TrimEnd('.'))
            .RuleFor(a => a.Description, f => f.Lorem.Paragraphs(1))
            .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(5))
            .RuleFor(a => a.Author, f => f.PickRandom(users))
            .RuleFor(a => a.CreatedAt, f => f.Date.Recent(90).ToUniversalTime())
            .RuleFor(a => a.Slug, (f, a) => _slugifier.Generate(a.Title))
            .Generate(500);

        foreach (var article in articles)
        {
            var f = new Faker();
            var comments = new Faker<Comment>()
                .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(2))
                .RuleFor(a => a.Author, f => f.PickRandom(users))
                .RuleFor(a => a.CreatedAt, f => f.Date.Recent(7).ToUniversalTime())
                .Generate(f.Random.Number(10)
            );

            foreach (var comment in comments)
            {
                article.AddComment(comment);
            }

            foreach (var tag in f.PickRandom(tags, f.Random.Number(3)))
            {
                article.AddTag(tag);
            }

            foreach (var user in f.PickRandom(users, f.Random.Number(5)))
            {
                article.Favorite(user);
            }
        }

        await _context.Articles.AddRangeAsync(articles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}