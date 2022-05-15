using Application.Interfaces;
using Application.Tools.Interfaces;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Tools.Seeders;

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

        await _context.Tags.AddRangeAsync(
            new Faker<Tag>()
                .RuleFor(a => a.Name, f => $"{f.Lorem.Word()}{f.UniqueIndex % 3}".TrimEnd('0'))
                .Generate(30),
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);

        var tags = await _context.Tags.ToListAsync(cancellationToken);

        var articles = new Faker<Article>()
            .RuleFor(a => a.Title, f => f.Lorem.Sentence().TrimEnd('.'))
            .RuleFor(a => a.Description, f => f.Lorem.Paragraphs(1))
            .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(5))
            .RuleFor(a => a.Author, f => f.PickRandom(users))
            .RuleFor(a => a.CreatedAt, f => f.Date.Recent(90).ToUniversalTime())
            .RuleFor(a => a.FavoredUsers, f => f.PickRandom(users, f.Random.Number(5))
                .Select(u => new ArticleFavorite { UserId = u.Id })
                .ToList()
            )
            .RuleFor(a => a.Slug, (f, a) => _slugifier.Generate(a.Title))
            .RuleFor(a => a.Comments, f => new Faker<Comment>()
                .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(2))
                .RuleFor(a => a.Author, f => f.PickRandom(users))
                .RuleFor(a => a.CreatedAt, f => f.Date.Recent(7).ToUniversalTime())
                .Generate(f.Random.Number(10))
            )
            .RuleFor(a => a.Tags, f => f.PickRandom(tags, f.Random.Number(3))
                .Select(t => new ArticleTag { TagId = t.Id })
                .ToList()
            )
            .Generate(500);

        await _context.Articles.AddRangeAsync(articles, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}