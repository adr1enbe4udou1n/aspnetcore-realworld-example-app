using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Tools.Interfaces;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Tools.Seeders
{
    public class ArticlesSeeder : ISeeder
    {
        private readonly IAppDbContext _context;

        public ArticlesSeeder(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var users = await _context.Users.ToListAsync(cancellationToken);

            var articles = new Faker<Article>()
                .RuleFor(a => a.Title, f => f.Lorem.Sentence())
                .RuleFor(a => a.Description, f => f.Lorem.Paragraphs(1))
                .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(5))
                .RuleFor(a => a.AuthorId, f => f.PickRandom(users).Id)
                .RuleFor(a => a.CreatedAt, f => f.Date.Recent(90))
                .RuleFor(a => a.FavoredUsers, f => f.PickRandom(users, f.Random.Number(5))
                    .Select(u => new ArticleFavorite { UserId = u.Id })
                    .ToList()
                )
                .Generate(500);

            await _context.Articles.AddRangeAsync(articles, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            await _context.Comments.AddRangeAsync(
                new Faker<Comment>()
                    .RuleFor(a => a.Body, f => f.Lorem.Paragraphs(2))
                    .RuleFor(a => a.ArticleId, f => f.PickRandom(articles).Id)
                    .RuleFor(a => a.AuthorId, f => f.PickRandom(users).Id)
                    .RuleFor(a => a.CreatedAt, f => f.Date.Recent(7))
                    .Generate(5000),
                cancellationToken
            );

            await _context.SaveChangesAsync(cancellationToken);

            await _context.Tags.AddRangeAsync(
                new Faker<Tag>()
                    .RuleFor(a => a.Name, f => f.Lorem.Word())
                    .RuleFor(a => a.Articles, f => f.PickRandom(articles, f.Random.Number(10))
                        .Select(a => new ArticleTag { ArticleId = a.Id })
                        .ToList()
                    )
                    .Generate(100),
                cancellationToken
            );

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}