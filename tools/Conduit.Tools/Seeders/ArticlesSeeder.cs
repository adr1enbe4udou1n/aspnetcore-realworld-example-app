using Bogus;

using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Tools.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Tools.Seeders;

public class ArticlesSeeder(IAppDbContext context, ISlugifier slugifier) : ISeeder
{
    public async Task Run(CancellationToken cancellationToken)
    {
        var users = await context.Users.ToListAsync(cancellationToken);

        var tags = new Faker<Tag>()
            .RuleFor(a => a.Name, f => $"{f.Lorem.Word()}{f.UniqueIndex % 10}".TrimEnd('0'))
            .Generate(30);

        await context.Tags.AddRangeAsync(tags, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);

        var articles = new Faker<Article>()
            .RuleFor(a => a.Title, f => f.Lorem.Sentence().TrimEnd('.'))
            .RuleFor(a => a.Description, f => f.Lorem.Paragraphs(1))
            .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(5))
            .RuleFor(a => a.Author, f => f.PickRandom(users))
            .RuleFor(a => a.CreatedAt, f => f.Date.Recent(90).ToUniversalTime())
            .RuleFor(a => a.Slug, (f, a) => slugifier.Generate(a.Title))
            .Generate(500);

        foreach (var article in articles)
        {
            var f = new Faker();

            foreach (var tag in f.PickRandom(tags, f.Random.Number(3)))
            {
                article.AddTag(tag);
            }

            foreach (var user in f.PickRandom(users, f.Random.Number(5)))
            {
                await context.ArticleFavorite.AddAsync(new ArticleFavorite
                {
                    Article = article,
                    User = user
                });
            }
        }

        var comments = new Faker<Comment>()
            .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(2))
            .RuleFor(a => a.Author, f => f.PickRandom(users))
            .RuleFor(a => a.CreatedAt, f => f.Date.Recent(7).ToUniversalTime())
            .RuleFor(a => a.Article, f => f.PickRandom(articles))
            .Generate(5000);

        await context.Articles.AddRangeAsync(articles, cancellationToken);
        await context.Comments.AddRangeAsync(comments, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}