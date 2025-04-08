using System.Net;

using Conduit.Application.Features.Articles.Queries;
using Conduit.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class ArticlesListTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Can_Paginate_Articles()
    {
        await CreateArticles();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=30&offset=10");

        Assert.Equal(20, response.Articles.Count());
        Assert.Equal(50, response.ArticlesCount);

        Assert.Equivalent(new
        {
            Title = "Jane Doe - Test Title 10",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "Jane Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag Jane Doe" },
        }, response.Articles.First());
    }

    [Fact]
    public async Task Can_Filter_Articles_By_Author()
    {
        await CreateArticles();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=10&offset=0&author=John Doe");

        Assert.Equal(10, response.Articles.Count());
        Assert.Equal(30, response.ArticlesCount);

        Assert.Equivalent(new
        {
            Title = "John Doe - Test Title 30",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "John Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
        }, response.Articles.First());
    }

    [Fact]
    public async Task Can_Filter_Articles_By_Tag()
    {
        await CreateArticles();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=10&offset=0&tag=Tag Jane Doe");

        Assert.Equal(10, response.Articles.Count());
        Assert.Equal(20, response.ArticlesCount);

        Assert.Equivalent(new
        {
            Title = "Jane Doe - Test Title 20",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "Jane Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag Jane Doe" },
        }, response.Articles.First());
    }

    [Fact]
    public async Task Can_Filter_Articles_By_Favorited()
    {
        await CreateArticles();

        var articles = new List<string>
            {
                "john-doe-test-title-1",
                "john-doe-test-title-2",
                "john-doe-test-title-4",
                "john-doe-test-title-8",
                "john-doe-test-title-16",
            };

        foreach (var a in articles)
        {
            Context.ArticleFavorite.Add(new ArticleFavorite
            {
                Article = await Context.Articles.FirstAsync(x => x.Slug == a),
                User = await Context.Users.FirstAsync(x => x.Name == "Jane Doe"),
            });
        }

        await Context.SaveChangesAsync();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=10&offset=0&favorited=Jane Doe");

        Assert.Equal(5, response.Articles.Count());
        Assert.Equal(5, response.ArticlesCount);

        Assert.Equivalent(new
        {
            Title = "John Doe - Test Title 16",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "John Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
            Favorited = true,
            FavoritesCount = 1
        }, response.Articles.First());
    }

    [Fact]
    public async Task Guest_Cannot_Paginate_Articles_Of_Followed_Authors()
    {
        var response = await Act(HttpMethod.Get, "/articles/feed");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Can_Paginate_Articles_Of_Followed_Authors()
    {
        await CreateArticles();

        Context.FollowerUser.Add(new FollowerUser
        {
            Follower = await Context.Users.FirstAsync(x => x.Name == "Jane Doe"),
            Following = await Context.Users.FirstAsync(x => x.Name == "John Doe"),
        });

        await Context.SaveChangesAsync();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles/feed?limit=10&offset=0");

        Assert.Equal(10, response.Articles.Count());
        Assert.Equal(30, response.ArticlesCount);

        Assert.Equivalent(new
        {
            Title = "John Doe - Test Title 30",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "John Doe",
                Following = true
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
        }, response.Articles.First());
    }

    private async Task CreateArticles()
    {
        await CreateArticlesForAuthor(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        }, 30);

        await CreateArticlesForAuthor(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        }, 20);
    }

    private async Task CreateArticlesForAuthor(User author, int count)
    {
        var user = await ActingAs(author);

        var articles = new List<string>();

        for (var i = 1; i <= count; i++)
        {
            articles.Add($"{author.Name} - Test Title {i}");
        }

        var tag1 = await Context.Tags
            .FirstOrDefaultAsync(x => x.Name == "Test Tag 1")
            ?? new Tag { Name = "Test Tag 1" };
        var tag2 = await Context.Tags
            .FirstOrDefaultAsync(x => x.Name == "Test Tag 2")
            ?? new Tag { Name = "Test Tag 2" };

        var tag3 = new Tag { Name = $"Tag {author.Name}" };
        Context.Tags.Add(tag3);

        foreach (var a in articles)
        {
            var article = new Article
            {
                Title = a,
                Description = "Test Description",
                Body = "Test Body",
                Slug = Slugifier.Generate(a),
                Author = user,
            };

            article.AddTag(tag1);
            article.AddTag(tag2);
            article.AddTag(tag3);

            Context.Articles.Add(article);
        }

        await Context.SaveChangesAsync();
    }
}